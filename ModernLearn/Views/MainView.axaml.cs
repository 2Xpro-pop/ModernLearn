using Avalonia.Controls;
using Avalonia.Interactivity;
using ModernLearn.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;

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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            topLevel.BackRequested += BackRequested;
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            topLevel.BackRequested -= BackRequested;
        }

        base.OnUnloaded(e);
    }

    private void BackRequested(object? sender, RoutedEventArgs e)
    {
        if (ViewModel?.Router.NavigationStack.Count > 1)
        {
            ViewModel.GoBackCommand.Execute().Subscribe();
            e.Handled = true;
        }
    }
}