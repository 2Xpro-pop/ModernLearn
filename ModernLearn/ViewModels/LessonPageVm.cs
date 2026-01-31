using Models = ModernLearnCore.DataAccess.Models;
using Controls = ModernLearn.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernLearn.ViewModels;

public sealed class LessonPageVm: PageVm
{
    public LessonPageVm(LessonVm lesson)
    {
        Lesson = lesson;
    }

    public LessonVm Lesson
    {
        get;
    }

    public static Controls.Lesson Parse(LessonVm lesson)
    {
        var control = SafeXamlLoader.Parse(lesson.Lesson.Xaml);

        if(control is not Controls.Lesson lessonControl)
        {
            throw new InvalidOperationException("The provided XAML does not represent a Lesson control.");
        }

        return lessonControl;
    }
}
