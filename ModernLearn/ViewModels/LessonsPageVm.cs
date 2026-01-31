using Avalonia.Controls;
using ModernLearnCore.DataAccess;
using ModernLearnCore.DataAccess.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Text;

namespace ModernLearn.ViewModels;

public sealed class LessonsPageVm : PageVm
{
    private readonly Course _course;

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public LessonsPageVm()
    {
        Debug.Assert(Design.IsDesignMode, "Only design-time data should be created.");

        _course = new(
            Guid.NewGuid(),
            "Sample Course",
            "This is a sample course description.",
            "Assets/SampleCourse.png",
            []
        );
    }

    public LessonsPageVm(Course course)
    {
        _course = course;
    }

    public ObservableCollection<LessonVm> Lessons { get; } = [];

    protected override void Initialize(CompositeDisposable disposables)
    {
        var lessonRepository = Locator.GetRequiredService<ILessonRepository>();

        var lessons = lessonRepository.GetLessonsByCourseId(_course.Id)
            .ToObservable()
            .Select(ToVm)
            .Subscribe(Lessons.Add)
            .DisposeWith(disposables);

    }

    private static LessonVm ToVm(Lesson lesson)
    {
        var image = ImageHelper.LoadFromResource($"Assets/{lesson.Id}.png");

        return new(lesson, image);
    }
}
