using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace ModernLearn.ViewModels;

public abstract partial class PageVm : ViewModelBase, IRoutableViewModel
{

    public PageVm(MainViewModel MainViewModel) : base(MainViewModel)
    {
        HostScreen = MainViewModel;

        GoBackCommand = ReactiveCommand.Create(() =>
        {
            MainViewModel.GoBackCommand.Execute().Subscribe();
        });
    }

    public PageVm() : this(Design.IsDesignMode ? Locator.GetService<MainViewModel>()! : Locator.GetRequiredService<MainViewModel>())
    {

    }

    public string? UrlPathSegment
    {
        get;
    }

    public IScreen HostScreen
    {
        get;
    }

    [Reactive]
    public partial string? BusyText
    {
        get; protected set;
    }

    public ReactiveCommand<Unit, Unit> GoBackCommand
    {
        get;
    }

    public async Task GoToPage(PageVm page)
    {
        await MainViewModel.GoToPage(page);
    }
}
