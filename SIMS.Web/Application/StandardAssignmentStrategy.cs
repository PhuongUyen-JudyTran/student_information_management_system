using System;
using System.Collections.Generic;
using System.Linq;
using SIMS.Domain.Entities;

namespace SIMS.Application
{
    public class StandardAssignmentStrategy : ICourseAssignmentStrategy
    {
        public bool CanApply(Student student)
        {
            if (student == null) return false;
            
            return !student.Program.Contains("Honors", StringComparison.OrdinalIgnoreCase);
        }

        public List<Enrollment> AssignCourses(Student student, List<Course> availableCourses)
        {
            if (student == null || availableCourses == null) return new List<Enrollment>();

            var enrollments = new List<Enrollment>();
            var targetPrefixes = student.Program.Contains("IT", StringComparison.OrdinalIgnoreCase) || 
                               student.Program.Contains("Information", StringComparison.OrdinalIgnoreCase) 
                               ? new[] { "CS", "SE" } 
                               : new[] { "BUS" };

            var matchingCourses = availableCourses
                .Where(c => targetPrefixes.Any(p => c.CourseId.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                .Take(3)
                .ToList();

            foreach (var course in matchingCourses)
            {
                enrollments.Add(new Enrollment
                {
                    StudentId = student.Id,
                    CourseId = course.CourseId,
                    EnrollmentDate = DateTime.Now,
                    Grade = "Pending"
                });
            }

            return enrollments;
        }
    }
}
