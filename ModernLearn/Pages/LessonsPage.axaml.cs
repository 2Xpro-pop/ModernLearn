using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ModernLearn.ViewModels;
using ReactiveUI.Avalonia;

namespace ModernLearn;

public partial class LessonsPage : ReactiveUserControl<LessonsPageVm>
{
    public LessonsPage()
    {
        InitializeComponent();
    }
}