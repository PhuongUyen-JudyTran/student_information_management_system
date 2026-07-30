using System.Collections.Generic;
using SIMS.Domain.Entities;

namespace SIMS.Application
{
    public interface ICourseAssignmentStrategy
    {
        bool CanApply(Student student);
        List<Enrollment> AssignCourses(Student student, List<Course> availableCourses);
    }
}
