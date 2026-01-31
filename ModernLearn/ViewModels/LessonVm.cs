using Avalonia.Media;
using ModernLearnCore.DataAccess.Models;

namespace ModernLearn.ViewModels;

public sealed record LessonVm(Lesson Lesson, IImage? Image);