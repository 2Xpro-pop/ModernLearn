using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using ModernLearn.Controls;
using TextMateSharp.Grammars;

namespace ModernLearn.Controls;

[TemplatePart("PART_TextEditor", typeof(TextEditor))]
public sealed class File : TemplatedControl
{
    public static readonly StyledProperty<string> FilePathProperty =
        AvaloniaProperty.Register<File, string>(nameof(FilePath));

    public static readonly StyledProperty<string> LanguageProperty =
        AvaloniaProperty.Register<File, string>(nameof(Language), "cs");

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<File, string>(nameof(Text));

    static File()
    {
        LanguageProperty.Changed.AddClassHandler<File>((x, e) => x.InstallLanguage());
        TextProperty.Changed.AddClassHandler<File>((x, e) => x.SetText());
    }

    private TextEditor? _textEditor;

    public string FilePath
    {
        get => GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }

    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    [Content]
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _textEditor = e.NameScope.Find<TextEditor>("PART_TextEditor");

        InstallLanguage();
        SetText();
    }

    private void InstallLanguage()
    {
        if (_textEditor is null)
        {
            return;
        }

        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        var textMateInstallation = _textEditor.InstallTextMate(registryOptions);

        textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension($".{Language}").Id));
    }

    private void SetText()
    {
        if (_textEditor is null)
        {
            return;
        }

        _textEditor.Text = Text.RemoveBaseIndent();
    }
}