using SIMS.Domain.Exceptions;

namespace SIMS.Domain.Entities
{
    public class AcademicRecord
    {
        public double CumulativeGPA { get; set; }
        public int TotalCreditsEarned { get; set; }

        public void Validate()
        {
            if (CumulativeGPA < 0.0 || CumulativeGPA > 4.0)
                throw new ValidationException("GPA must be between 0.0 and 4.0.");

            if (TotalCreditsEarned < 0)
                throw new ValidationException("Total credits earned cannot be negative.");
        }
    }
}
