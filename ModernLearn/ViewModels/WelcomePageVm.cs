using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

namespace ModernLearn.ViewModels;

public sealed class WelcomePageVm: PageVm
{
    public WelcomePageVm()
    {
        StartCommand = ReactiveCommand.CreateFromTask(async () => await GoToPage(new CoursesPageVm()));
    }

    public ReactiveCommand<Unit, Unit> StartCommand
    {
        get;
    }
}
