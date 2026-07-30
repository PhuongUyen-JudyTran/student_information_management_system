using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SIMS.Infrastructure.Locking;

namespace SIMS.Infrastructure.Data
{
    public class CsvContext
    {
        private readonly string _dataFolder;
        private readonly ICsvFileLockManager _lockManager;

        public CsvContext(IConfiguration configuration, ICsvFileLockManager lockManager)
        {
            _lockManager = lockManager;
            
            var configPath = configuration["DataSettings:CsvFolder"];
            if (!string.IsNullOrWhiteSpace(configPath))
            {
                _dataFolder = Path.IsPathRooted(configPath) 
                    ? configPath 
                    : Path.Combine(AppContext.BaseDirectory, configPath);
            }
            else
            {
                _dataFolder = Path.Combine(AppContext.BaseDirectory, "Data");
            }

            if (!Directory.Exists(_dataFolder))
            {
                Directory.CreateDirectory(_dataFolder);
            }
        }

        public CsvContext(string dataFolder, ICsvFileLockManager lockManager)
        {
            _dataFolder = dataFolder;
            _lockManager = lockManager;
            
            if (!Directory.Exists(_dataFolder))
            {
                Directory.CreateDirectory(_dataFolder);
            }
        }

        public string GetDataFolder() => _dataFolder;

        public string GetFilePath(string filename)
        {
            return Path.Combine(_dataFolder, filename);
        }

        public async Task<List<string>> ReadLinesAsync(string filename)
        {
            var filepath = GetFilePath(filename);

            using (await _lockManager.AcquireLockAsync(filename))
            {
                if (!File.Exists(filepath))
                {
                    return new List<string>();
                }

                var lines = await File.ReadAllLinesAsync(filepath);
                return new List<string>(lines);
            }
        }

        public async Task WriteAllLinesAsync(string filename, IEnumerable<string> lines)
        {
            var filepath = GetFilePath(filename);
            var tempPath = filepath + ".tmp";

            using (await _lockManager.AcquireLockAsync(filename))
            {
                await File.WriteAllLinesAsync(tempPath, lines);

                if (File.Exists(filepath))
                {
                    File.Move(tempPath, filepath, overwrite: true);
                }
                else
                {
                    File.Move(tempPath, filepath);
                }
            }
        }
    }
}
