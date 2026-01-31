using Avalonia.Media;
using ModernLearnCore.DataAccess.Models;
using System.Windows.Input;

namespace ModernLearn.ViewModels;

public sealed record LessonVm(Lesson Lesson, IImage? Image, ICommand Command);