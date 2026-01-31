using ModernLearnCore.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace ModernLearnCore.DataAccess;

public sealed class XmlLessonRepository : ILessonRepository
{
    public static readonly XmlLessonRepository Default =
        new(
            courseRepository: XmlCourseRepository.Default,
            resourcePrefix: "ModernLearnCore.DataAccess.Data.Lessons.",
            assembly: typeof(XmlLessonRepository).Assembly);

    private readonly ICourseRepository _courseRepository;
    private readonly Assembly _assembly;
    private readonly string _resourcePrefix;

    private readonly SemaphoreSlim _indexGate = new(1, 1);
    private ImmutableDictionary<Guid, string>? _baseResourceNameByLessonId;

    private readonly Dictionary<string, string> _resourceNameLookupByLower;
    private readonly Lock _resourceTextCacheGate = new();
    private readonly Dictionary<string, string> _resourceTextCacheByResourceName = new(StringComparer.OrdinalIgnoreCase);

    private XmlLessonRepository(
        ICourseRepository courseRepository,
        string resourcePrefix,
        Assembly? assembly = null)
    {
        _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
        _resourcePrefix = resourcePrefix ?? throw new ArgumentNullException(nameof(resourcePrefix));
        _assembly = assembly ?? Assembly.GetExecutingAssembly();

        var manifestResourceNames = _assembly.GetManifestResourceNames();

        _resourceNameLookupByLower = manifestResourceNames
            .GroupBy(static resourceName => resourceName.ToLowerInvariant(), StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);
    }

    public async Task<Lesson?> GetLessonById(Guid id)
    {
        var baseResourceNameByLessonId = await GetIndexAsync().ConfigureAwait(false);

        if (!baseResourceNameByLessonId.TryGetValue(id, out var baseResourceName))
        {
            return null;
        }

        var chosenResourceName =
            FindBestLocalizedResourceName(baseResourceName, CultureInfo.CurrentUICulture) ??
            baseResourceName;

        var lessonXmlText = await LoadResourceTextAsync(chosenResourceName).ConfigureAwait(false);
        if (lessonXmlText is null)
        {
            // Fallback to base if localization resource exists but can't be read for some reason.
            if (!string.Equals(chosenResourceName, baseResourceName, StringComparison.OrdinalIgnoreCase))
            {
                lessonXmlText = await LoadResourceTextAsync(baseResourceName).ConfigureAwait(false);
            }
        }

        if (lessonXmlText is null)
        {
            return null;
        }

        return new Lesson(id, lessonXmlText);
    }

    public async IAsyncEnumerable<Lesson> GetLessons()
    {
        var baseResourceNameByLessonId = await GetIndexAsync().ConfigureAwait(false);

        foreach (var lessonId in baseResourceNameByLessonId.Keys.OrderBy(static value => value))
        {
            var lesson = await GetLessonById(lessonId).ConfigureAwait(false);

            if (lesson is not null)
            {
                yield return lesson;
            }
        }
    }

    public async IAsyncEnumerable<Lesson> GetLessonsByCourseId(Guid courseId)
    {
        var course = await _courseRepository.GetCourseById(courseId).ConfigureAwait(false);
        if (course is null)
        {
            yield break;
        }

        foreach (var lessonId in course.LessonIds)
        {
            var lesson = await GetLessonById(lessonId).ConfigureAwait(false);

            if (lesson is not null)
            {
                yield return lesson;
            }
        }
    }

    private async Task<ImmutableDictionary<Guid, string>> GetIndexAsync()
    {
        if (_baseResourceNameByLessonId is not null)
        {
            return _baseResourceNameByLessonId;
        }

        await _indexGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_baseResourceNameByLessonId is not null)
            {
                return _baseResourceNameByLessonId;
            }

            var builder = ImmutableDictionary.CreateBuilder<Guid, string>();

            foreach (var resourceName in _assembly.GetManifestResourceNames())
            {
                if (!resourceName.StartsWith(_resourcePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!resourceName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var lessonId = await TryReadLessonIdAsync(resourceName).ConfigureAwait(false);
                if (lessonId is null)
                {
                    // This is likely a localized file without Id.
                    continue;
                }

                var id = lessonId.Value;

                if (!builder.TryGetValue(id, out var existingBaseResourceName))
                {
                    builder.Add(id, resourceName);
                    continue;
                }

                // If duplicates exist, prefer the non-localized resource as the "base".
                var existingIsLocalized = LooksLikeLocalizedResource(existingBaseResourceName);
                var currentIsLocalized = LooksLikeLocalizedResource(resourceName);

                if (existingIsLocalized && !currentIsLocalized)
                {
                    builder[id] = resourceName;
                }
            }

            _baseResourceNameByLessonId = builder.ToImmutable();
            return _baseResourceNameByLessonId;
        }
        finally
        {
            _indexGate.Release();
        }
    }

    private async Task<Guid?> TryReadLessonIdAsync(string resourceName)
    {
        await using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        try
        {
            var xDocument = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
            var root = xDocument.Root;

            if (root is null || !string.Equals(root.Name.LocalName, "Lesson", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var idAttribute = root.Attribute("Id");
            if (idAttribute is null)
            {
                return null;
            }

            if (!Guid.TryParse(idAttribute.Value.Trim('{', '}'), out var id))
            {
                return null;
            }

            return id;
        }
        catch
        {
            return null;
        }
    }

    private string? FindBestLocalizedResourceName(string baseResourceName, CultureInfo uiCulture)
    {
        if (!baseResourceName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var baseWithoutExtension = baseResourceName[..^4];

        // Build culture fallback chain: ru-RU -> ru -> (parent...) -> fallback
        var culture = uiCulture;
        var seenTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (!string.IsNullOrWhiteSpace(culture.Name) && !Equals(culture, CultureInfo.InvariantCulture))
        {
            var specificToken = culture.Name.ToLowerInvariant(); // e.g. "ru-RU" -> "ru-ru"
            if (seenTokens.Add(specificToken))
            {
                var candidate = $"{baseWithoutExtension}.{specificToken}.xml";
                var resolved = ResolveResourceName(candidate);
                if (resolved is not null)
                {
                    return resolved;
                }
            }

            var neutralToken = culture.TwoLetterISOLanguageName.ToLowerInvariant(); // e.g. "ru"
            if (seenTokens.Add(neutralToken))
            {
                var candidate = $"{baseWithoutExtension}.{neutralToken}.xml";
                var resolved = ResolveResourceName(candidate);
                if (resolved is not null)
                {
                    return resolved;
                }
            }

            culture = culture.Parent;
        }

        return null;
    }

    private string? ResolveResourceName(string candidateResourceName)
    {
        var lower = candidateResourceName.ToLowerInvariant();
        return _resourceNameLookupByLower.TryGetValue(lower, out var actual) ? actual : null;
    }

    private async Task<string?> LoadResourceTextAsync(string resourceName)
    {
        lock (_resourceTextCacheGate)
        {
            if (_resourceTextCacheByResourceName.TryGetValue(resourceName, out var cachedText))
            {
                return cachedText;
            }
        }

        await using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
        var text = await reader.ReadToEndAsync().ConfigureAwait(false);

        lock (_resourceTextCacheGate)
        {
            _resourceTextCacheByResourceName[resourceName] = text;
        }

        return text;
    }

    private static bool LooksLikeLocalizedResource(string resourceName)
    {
        // Checks the filename tail: "...something.<culture>.xml"
        // Culture token patterns: "ru", "en", "ru-ru", "en-us", etc.
        if (!resourceName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var withoutExtension = resourceName[..^4];
        var lastDotIndex = withoutExtension.LastIndexOf('.');
        if (lastDotIndex < 0)
        {
            return false;
        }

        var token = withoutExtension[(lastDotIndex + 1)..];
        if (token.Length == 2 && token.All(static ch => ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z'))
        {
            return true;
        }

        // "xx-yy" pattern
        if (token.Length == 5 &&
            char.IsLetter(token[0]) &&
            char.IsLetter(token[1]) &&
            token[2] == '-' &&
            char.IsLetter(token[3]) &&
            char.IsLetter(token[4]))
        {
            return true;
        }

        return false;
    }
}