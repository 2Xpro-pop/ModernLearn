using Avalonia.Media;
using ModernLearnCore.DataAccess.Models;

namespace ModernLearn.ViewModels;

public sealed record CourseVm(Course Course, IImage? Image);
