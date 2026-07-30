using System;
using System.Collections.Generic;
using SIMS.Domain.Exceptions;

namespace SIMS.Domain.Entities
{
    public class Faculty : User
    {
        public string Department { get; set; } = string.Empty;
        public List<Course> InstructedCourses { get; set; } = new List<Course>();

        public Faculty()
        {
            Role = UserRole.Faculty;
        }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Department))
                throw new ValidationException("Department is required.");

            if (InstructedCourses == null)
                throw new ValidationException("Instructed courses list cannot be null.");

            foreach (var course in InstructedCourses)
            {
                course.Validate();
            }
        }
    }
}
