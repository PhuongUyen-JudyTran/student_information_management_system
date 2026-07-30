using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SIMS.Infrastructure.Locking
{
    public class CsvFileLockManager : ICsvFileLockManager
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = 
            new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        public async Task<IDisposable> AcquireLockAsync(string filename)
        {
            var sem = _locks.GetOrAdd(filename, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            return new Releaser(sem);
        }

        private class Releaser : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            private bool _disposed;

            public Releaser(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _semaphore.Release();
                    _disposed = true;
                }
            }
        }
    }
}
