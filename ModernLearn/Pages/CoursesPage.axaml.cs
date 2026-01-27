using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using ModernLearn.ViewModels;
using ReactiveUI.Avalonia;

namespace ModernLearn;

public partial class CoursesPage : ReactiveUserControl<CoursesPageVm>
{
    public CoursesPage()
    {
        InitializeComponent();
    }
}