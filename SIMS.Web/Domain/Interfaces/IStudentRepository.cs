using System.Collections.Generic;
using System.Threading.Tasks;
using SIMS.Domain.Entities;

namespace SIMS.Domain.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student?> GetByIdAsync(string id);
        Task<List<Student>> GetAllAsync();
        Task<bool> AddAsync(Student student);
        Task<bool> UpdateAsync(Student student);
        Task<bool> DeleteAsync(string id);
    }
}
