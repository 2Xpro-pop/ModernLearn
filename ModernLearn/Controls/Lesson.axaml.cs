using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace ModernLearn.Controls;

public class Lesson : TemplatedControl
{
    private StackPanel? _stackPanel = null;
    private readonly Avalonia.Controls.Controls _childrenFallback = [];

    [Content]
    public Avalonia.Controls.Controls Children => _stackPanel?.Children ?? _childrenFallback;

    public Lesson()
    {

    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _stackPanel = e.NameScope.Find<StackPanel>("PART_StackPanel");

        _stackPanel?.Children.AddRange(_childrenFallback);
    }
}