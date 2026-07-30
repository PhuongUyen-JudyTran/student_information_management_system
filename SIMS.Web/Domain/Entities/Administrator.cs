using SIMS.Domain.Exceptions;

namespace SIMS.Domain.Entities
{
    public class Administrator : User
    {
        public string AdminLevel { get; set; } = string.Empty;

        public Administrator()
        {
            Role = UserRole.Administrator;
        }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(AdminLevel))
                throw new ValidationException("Administrator level is required.");
        }
    }
}
