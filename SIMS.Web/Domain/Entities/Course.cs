using SIMS.Domain.Exceptions;

namespace SIMS.Domain.Entities
{
    public class Course
    {
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int MaxCapacity { get; set; }
        public string FacultyId { get; set; } = string.Empty;
        public decimal Fee { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(CourseId))
                throw new ValidationException("Course ID is required.");

            if (string.IsNullOrWhiteSpace(Title))
                throw new ValidationException("Course Title is required.");

            if (Credits <= 0 || Credits > 6)
                throw new ValidationException("Course credits must be between 1 and 6.");

            if (MaxCapacity <= 0)
                throw new ValidationException("Maximum capacity must be greater than 0.");

            if (Fee < 0)
                throw new ValidationException("Course fee cannot be negative.");
        }
    }
}
