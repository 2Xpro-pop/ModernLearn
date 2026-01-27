using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;

namespace ModernLearn.ViewModels;

public abstract partial class ViewModelBase : ReactiveObject, IActivatableViewModel, ICancelable, IInitializableViewModel, IAsyncDisposable
{
    protected CompositeDisposable InternalDisposables
    {
        get;
    } = [];

    [Reactive]
    public partial bool IsDisposed
    {
        get; protected set;
    }

    protected static IReadonlyDependencyResolver Locator => Splat.Locator.Current;

    public MainViewModel MainViewModel
    {
        get;
    }

    public ViewModelActivator Activator
    {
        get;
    }

    public ViewModelBase() : this(Locator.GetRequiredService<MainViewModel>())
    {

    }

    public ViewModelBase(MainViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;

        Activator = new();

        this.WhenActivated(OnActivated);

        Activator.DisposeWith(InternalDisposables);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {

            if (disposing)
            {
                InternalDisposables.Dispose();
            }

        }
    }

    public ValueTask InitializeAsync()
    {
        Initialize(InternalDisposables);
        return InitializeAsync(InternalDisposables);
    }

    protected virtual ValueTask InitializeAsync(CompositeDisposable disposables)
    {
        return ValueTask.CompletedTask;
    }

    protected virtual void Initialize(CompositeDisposable disposables)
    {

    }

    protected virtual void OnActivated(CompositeDisposable disposables)
    {

    }

    public void Dispose()
    {
        Debug.WriteLine($"Disposing {GetType().Name}");
        Dispose(disposing: true);
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    protected virtual ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();

        Dispose(false);
        IsDisposed = true;

        GC.SuppressFinalize(this);
    }
}
