using System.Collections.Generic;
using System.Threading.Tasks;
using SIMS.Domain.Entities;

namespace SIMS.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> AddAsync(User user);
        Task<bool> ValidateCredentialsAsync(string username, string passwordHash);
        Task<List<User>> GetAllAsync();
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(string id);
    }
}
