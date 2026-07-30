using System.Collections.Generic;
using System.Threading.Tasks;
using SIMS.Domain.Entities;

namespace SIMS.Domain.Interfaces
{
    public interface IEnrollmentRepository
    {
        Task<List<Enrollment>> GetByStudentIdAsync(string studentId);
        Task<List<Enrollment>> GetByCourseIdAsync(string courseId);
        Task<bool> AddAsync(Enrollment enrollment);
        Task<bool> DeleteAsync(string studentId, string courseId);
    }
}
