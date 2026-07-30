using System;
using SIMS.Domain.Exceptions;

namespace SIMS.Domain.Entities
{
    public abstract class User
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        public virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ValidationException("User ID is required.");

            if (string.IsNullOrWhiteSpace(Username))
                throw new ValidationException("Username is required.");

            if (Username.Length < 3)
                throw new ValidationException("Username must be at least 3 characters long.");

            if (string.IsNullOrWhiteSpace(PasswordHash))
                throw new ValidationException("Password is required.");

            if (string.IsNullOrWhiteSpace(Email))
                throw new ValidationException("Email is required.");

            if (!Email.Contains("@") || !Email.Contains("."))
                throw new ValidationException("Invalid email format.");
        }
    }
}
