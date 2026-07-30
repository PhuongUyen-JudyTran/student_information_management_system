using System.Collections.Generic;
using System.Threading.Tasks;
using SIMS.Domain.Entities;

namespace SIMS.Domain.Interfaces
{
    public interface ICourseRepository
    {
        Task<Course?> GetByIdAsync(string id);
        Task<List<Course>> GetAllAsync();
        Task<bool> AddAsync(Course course);
        Task<bool> UpdateAsync(Course course);
        Task<bool> DeleteAsync(string id);
    }
}
