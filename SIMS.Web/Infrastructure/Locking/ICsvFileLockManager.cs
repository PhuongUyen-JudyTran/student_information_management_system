using System;
using System.Threading.Tasks;

namespace SIMS.Infrastructure.Locking
{
    public interface ICsvFileLockManager
    {
        Task<IDisposable> AcquireLockAsync(string filename);
    }
}
