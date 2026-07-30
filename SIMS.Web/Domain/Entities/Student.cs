using System;
using System.Collections.Generic;
using SIMS.Domain.Exceptions;

namespace SIMS.Domain.Entities
{
    public class Student : User
    {
        public DateTime DateOfBirth { get; set; }
        public string Program { get; set; } = string.Empty;
        public AcademicRecord Record { get; set; } = new AcademicRecord();
        public List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public Student()
        {
            Role = UserRole.Student;
        }

        public override void Validate()
        {
            base.Validate();

            if (DateOfBirth == default)
                throw new ValidationException("Date of birth is required.");

            var today = DateTime.Today;
            int age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;

            if (age < 16)
                throw new ValidationException("Student must be at least 16 years old.");

            if (string.IsNullOrWhiteSpace(Program))
                throw new ValidationException("Academic program is required.");

            if (Record == null)
                throw new ValidationException("Academic record is required.");

            Record.Validate();

            if (Enrollments == null)
                throw new ValidationException("Enrollments list cannot be null.");

            foreach (var enrollment in Enrollments)
            {
                enrollment.Validate();
            }
        }
    }
}
