using Avalonia.Controls;
using DynamicData;
using ModernLearnCore.DataAccess;
using ModernLearnCore.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernLearn.ViewModels;

public sealed class CoursesPageVm : PageVm
{
    public ObservableCollection<CourseVm> Courses { get; } = [];

    public CoursesPageVm()
    {
#if DEBUG
        if (Design.IsDesignMode)
        {
            Course[] courses = [
                new(
                    Guid.Parse("EB68DFFB-360E-41B1-966D-D626A4A15E82"),
                    "Avalonia",
                    "Avalonia is modern ui framework.",
                    "Assets/Avalonia.png",
                    []
                ),
                new(
                    Guid.Parse("A12C3F45-1E09-4A77-9F53-8B2D25982D90"),
                    "C# Fundamentals",
                    "Learn the basics of C#, syntax, and core programming concepts.",
                    "Assets/CSFundamentals.png",
                    []
                ),
                new(
                    Guid.Parse("5F77EA0A-2A0C-48CA-9C5F-3AF7411B3A28"),
                    "Reactive Extensions",
                    "Master reactive programming with Rx.NET.",
                    "Assets/Rx.png",
                    []
                ),
                new(
                    Guid.Parse("9B4D5AE2-7C9E-4C23-8A8D-52158D0B6A77"),
                    "Entity Framework Core",
                    "Understand how to work with EF Core and build data-driven applications.",
                    "Assets/EFCore.png",
                    []
                ),
                new(
                    Guid.Parse("C2EAC1AF-4A29-4E34-9547-588D6C514A7E"),
                    "Git and GitHub",
                    "Learn version control with Git and collaboration via GitHub.",
                    "Assets/Git.png",
                    []
                )
            ];

            Courses.AddRange(courses.Select(ToVmSync));
        }
#endif
    }

    protected override void Initialize(CompositeDisposable disposables)
    {
        var repository = Locator.GetRequiredService<ICourseRepository>();

        repository.GetCourses()
            .ToObservable()
            .SelectMany(ToVm)
            .Subscribe(Courses.Add)
            .DisposeWith(disposables);
    }

    private static async Task<CourseVm> ToVm(Course course)
    {
        if (course.ImageName.StartsWith("http"))
        {
            var image = await ImageHelper.LoadFromWeb(new Uri(course.ImageName));
            return new CourseVm(course, image);
        }

        var uri = new Uri($"avares://ModernLearn/{course.ImageName}");

        return new CourseVm(course, ImageHelper.LoadFromResource(uri));
    }

    private static CourseVm ToVmSync(Course course)
    {
        var uri = new Uri($"avares://ModernLearn/{course.ImageName}");

        return new CourseVm(course, ImageHelper.LoadFromResource(uri));
    }
}