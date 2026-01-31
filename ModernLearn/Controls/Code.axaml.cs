using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace ModernLearn.Controls;

public sealed class Code : TemplatedControl
{
    public static readonly StyledProperty<string> LanguageProperty =
        AvaloniaProperty.Register<Code, string>(nameof(Language), "cs");

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<Code, string>(nameof(Text), string.Empty);

    static Code()
    {
        LanguageProperty.Changed.AddClassHandler<Code>((x, e) => x.InstallLanguage());
        TextProperty.Changed.AddClassHandler<Code>((x, e) => x.SetText());
    }

    private TextEditor? _textEditor;

    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

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
        if(_textEditor is null)
        {
            return;
        }

        _textEditor.Text = Text.RemoveBaseIndent();
    }
}