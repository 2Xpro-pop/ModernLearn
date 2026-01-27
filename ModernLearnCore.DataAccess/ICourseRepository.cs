using ModernLearnCore.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernLearnCore.DataAccess;

public interface ICourseRepository
{
    public IAsyncEnumerable<Course> GetCourses();
}
