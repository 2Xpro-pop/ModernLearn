using ModernLearnCore.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace ModernLearnCore.DataAccess;

public sealed class XmlCourseRepository : ICourseRepository
{
    public static readonly XmlCourseRepository Default = new("ModernLearnCore.DataAccess.Data.Courses.xml", typeof(XmlCourseRepository).Assembly);

    private readonly Assembly _assembly;
    private readonly string _resourceName;

    private XmlCourseRepository(string resourceName, Assembly? assembly = null)
    {
        _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
        _assembly = assembly ?? Assembly.GetExecutingAssembly();
    }

    public async IAsyncEnumerable<Course> GetCourses()
    {
        await using var stream = _assembly.GetManifestResourceStream(_resourceName);
        if (stream is null)
        {
            yield break;
        }

        var xdoc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

        var root = xdoc.Root;
        if (root is null || root.Name != "Courses")
        {
            yield break;
        }

        foreach (var courseElement in root.Elements("Course"))
        {
            var idAttr = courseElement.Attribute("Id");
            var titleAttr = courseElement.Attribute("Title");
            var descriptionAttr = courseElement.Attribute("Description");
            var imageNameAttr = courseElement.Attribute("ImageName");

            if (idAttr is null || titleAttr is null || descriptionAttr is null || imageNameAttr is null)
            {
                continue;
            }

            if (!Guid.TryParse(idAttr.Value.Trim('{', '}'), out var id))
            {
                continue;
            }

            var name = titleAttr.Value;
            var imageName = imageNameAttr.Value;
            var description = descriptionAttr.Value;

            var lessonIds = ParseLessonIds(courseElement);

            yield return new Course(id, name, description, imageName, lessonIds);
        }
    }

    private static ImmutableArray<Guid> ParseLessonIds(XElement courseElement)
    {
        var builder = ImmutableArray.CreateBuilder<Guid>();
        var lessons = courseElement.Element("Lessons");

        if (lessons is not null)
        {
            foreach (var lesson in lessons.Elements("Lesson"))
            {
                var idAttr = lesson.Attribute("Id");
                if (idAttr is not null &&
                    Guid.TryParse(idAttr.Value.Trim('{', '}'), out var id))
                {
                    builder.Add(id);
                }
            }
        }

        return builder.ToImmutable();
    }
}
