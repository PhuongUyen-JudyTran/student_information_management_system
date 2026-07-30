using System;
using SIMS.Domain.Exceptions;

namespace SIMS.Domain.Entities
{
    public class Enrollment
    {
        public string StudentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public string Grade { get; set; } = "Pending";
        public bool IsPaid { get; set; } = false;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(StudentId))
                throw new ValidationException("Student ID is required for enrollment.");

            if (string.IsNullOrWhiteSpace(CourseId))
                throw new ValidationException("Course ID is required for enrollment.");

            if (EnrollmentDate == default)
                throw new ValidationException("Enrollment date is required.");
        }
    }
}
