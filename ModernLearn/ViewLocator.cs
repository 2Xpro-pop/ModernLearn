using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ModernLearn.Pages;
using ModernLearn.ViewModels;
using ReactiveUI;

namespace ModernLearn;

public sealed class ViewLocator : IViewLocator
{
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null) => viewModel switch
    {
        WelcomePageVm => new WelcomePage() { DataContext = viewModel },
        CoursesPageVm => new CoursesPage() { DataContext = viewModel },
        LessonsPageVm => new LessonsPage() { DataContext = viewModel },
        LessonPageVm lesson => LessonPageVm.Parse(lesson.Lesson).SetVm(lesson),
        _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
    };
}