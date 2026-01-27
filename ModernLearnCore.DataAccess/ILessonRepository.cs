using ModernLearnCore.DataAccess.Models;

namespace ModernLearnCore.DataAccess;

public interface ILessonRepository
{
    public Task<Lesson?> GetLessonById(Guid id);

    public IAsyncEnumerable<Lesson?> GetLessons();

    public IAsyncEnumerable<Lesson?> GetLessonsByCourseId(Guid courseId);
}
