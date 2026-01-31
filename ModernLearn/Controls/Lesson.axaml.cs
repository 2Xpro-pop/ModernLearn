using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using ModernLearn.ViewModels;
using ReactiveUI;

namespace ModernLearn.Controls;

public class Lesson : TemplatedControl, IViewFor<LessonPageVm>
{
    private StackPanel? _stackPanel = null;
    private readonly Avalonia.Controls.Controls _childrenFallback = [];

    [Content]
    public Avalonia.Controls.Controls Children => _stackPanel?.Children ?? _childrenFallback;

    public LessonPageVm? ViewModel 
    { 
        get => DataContext as LessonPageVm;
        set => DataContext = value;
    }

    object? IViewFor.ViewModel 
    { 
        get => ViewModel; 
        set => ViewModel = (LessonPageVm?)value; 
    }

    public Lesson()
    {

    }

    public Lesson SetVm(LessonPageVm viewModel)
    {
        ViewModel = viewModel;
        return this;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _stackPanel = e.NameScope.Find<StackPanel>("PART_StackPanel");

        _stackPanel?.Children.AddRange(_childrenFallback);
    }
}