
using ReactiveUI;
using Splat;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ModernLearn.ViewModels;

public sealed class MainViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new();

    public ReactiveCommand<PageVm, PageVm> GoToPageCommand
    {
        get;
    }

    public ReactiveCommand<PageVm, PageVm> GoToPageAndResetCommand
    {
        get;
    }

    public ReactiveCommand<Unit, Unit> GoBackCommand
    {
        get;
    }

    public MainViewModel()
    {
        Locator.CurrentMutable.RegisterConstant(this);

        Router = new();
        Router.Navigate.Execute(new WelcomePageVm());

        GoToPageCommand = ReactiveCommand.CreateFromTask(async (PageVm pageVm) =>
        {
            await pageVm.InitializeAsync();

            await Router.Navigate.Execute(pageVm).FirstAsync();

            return pageVm;
        });

        GoToPageAndResetCommand = ReactiveCommand.CreateFromTask(async (PageVm pageVm) =>
        {
            foreach (var vm in Router.NavigationStack)
            {
                if (vm is PageVm page)
                {
                    await page.DisposeAsync();
                }
            }

            await pageVm.InitializeAsync();

            Router.NavigationStack.Clear();

            await Router.Navigate.Execute(pageVm).FirstAsync();

            return pageVm;
        });

        GoBackCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (Router.NavigationStack.Count > 1)
            {
                var currentPage = Router.NavigationStack[^1];
                Router.NavigationStack.Remove(currentPage);

                if (currentPage is PageVm pageVm)
                {
                    pageVm.Activator.Deactivate();

                    await pageVm.DisposeAsync();
                }
            }
        });

    }
    public async Task GoToPage(PageVm pageVm)
    {
        await GoToPageCommand.Execute(pageVm).FirstAsync();
    }

    public async Task GoToPageAndReset(PageVm pageVm)
    {
        await GoToPageAndResetCommand.Execute(pageVm).FirstAsync();
    }
}
