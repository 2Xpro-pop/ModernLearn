using Avalonia.Controls;
using ModernLearn.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ModernLearn.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        this.WhenActivated(disposables => 
        {
            Router.Router = ViewModel?.Router;
        });
        InitializeComponent();
    }
}