using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ModernLearn.ViewModels;
using ReactiveUI.Avalonia;

namespace ModernLearn.Pages;

public partial class WelcomePage : ReactiveUserControl<WelcomePageVm>
{
    public WelcomePage()
    {
        InitializeComponent();
    }
}