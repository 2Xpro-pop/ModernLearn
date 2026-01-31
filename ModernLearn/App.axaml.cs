using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ModernLearn.ViewModels;
using ModernLearn.Views;
using ModernLearnCore.DataAccess;
using Splat;

namespace ModernLearn;

public partial class App : Application
{
    public App()
    {
        var culture = System.Globalization.CultureInfo.CurrentUICulture;

        Locator.CurrentMutable.RegisterConstant<ICourseRepository>(XmlCourseRepository.Default);
        Locator.CurrentMutable.RegisterConstant<ILessonRepository>(XmlLessonRepository.Default);
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}