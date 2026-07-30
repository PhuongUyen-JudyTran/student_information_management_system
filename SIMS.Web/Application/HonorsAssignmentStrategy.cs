using System;
using System.Collections.Generic;
using System.Linq;
using SIMS.Domain.Entities;

namespace SIMS.Application
{
    public class HonorsAssignmentStrategy : ICourseAssignmentStrategy
    {
        public bool CanApply(Student student)
        {
            if (student == null) return false;

            return student.Program.Contains("Honors", StringComparison.OrdinalIgnoreCase) || 
                   (student.Record != null && student.Record.CumulativeGPA >= 3.5);
        }

        public List<Enrollment> AssignCourses(Student student, List<Course> availableCourses)
        {
            if (student == null || availableCourses == null) return new List<Enrollment>();

            var enrollments = new List<Enrollment>();
            
            var advancedCourses = availableCourses
                .Where(c => c.Credits >= 4 || 
                            c.CourseId.Any(ch => char.IsDigit(ch) && (ch == '3' || ch == '4' || ch == '5')))
                .Take(4)
                .ToList();

            if (advancedCourses.Count < 4)
            {
                var otherCourses = availableCourses
                    .Where(c => !advancedCourses.Contains(c))
                    .Take(4 - advancedCourses.Count);
                advancedCourses.AddRange(otherCourses);
            }

            foreach (var course in advancedCourses)
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
