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

    static Code()
    {
        LanguageProperty.Changed.AddClassHandler<Code>((x, e) => x.InstallLanguage());
    }

    private TextEditor? _textEditor;

    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _textEditor = e.NameScope.Find<TextEditor>("PART_TextEditor");

        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        var textMateInstallation = _textEditor.InstallTextMate(registryOptions);

        textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension($".{Language}").Id));
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
}