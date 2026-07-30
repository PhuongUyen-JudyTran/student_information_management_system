using System;
using SIMS.Domain.Entities;

namespace SIMS.Domain.Factories
{
    public class UserFactory
    {
        public User CreateUser(UserRole role, string id, string username, string email)
        {
            User user = role switch
            {
                UserRole.Student => new Student(),
                UserRole.Faculty => new Faculty(),
                UserRole.Administrator => new Administrator(),
                _ => throw new ArgumentException($"Unsupported user role: {role}", nameof(role))
            };

            user.Id = id;
            user.Username = username;
            user.Email = email;
            return user;
        }
    }
}
