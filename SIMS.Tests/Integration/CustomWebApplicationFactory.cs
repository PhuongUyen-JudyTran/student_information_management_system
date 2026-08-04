using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SIMS.Tests.Integration
{
    // Custom factory that allows overriding the CSV data folder for isolation in tests
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
    {
        public string DataFolder { get; }

        private bool _disposed;

        public CustomWebApplicationFactory()
        {
            // Create a unique temporary folder per factory instance
            DataFolder = Path.Combine(Path.GetTempPath(), "SIMS_Test_Data_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DataFolder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                var settings = new System.Collections.Generic.Dictionary<string, string>
                {
                    // Use absolute path so CsvContext will accept it as rooted path
                    ["DataSettings:CsvFolder"] = Path.GetFullPath(DataFolder)
                };

                conf.AddInMemoryCollection(settings);
            });

            base.ConfigureWebHost(builder);
        }

        public new void Dispose()
        {
            if (_disposed) return;

            try
            {
                base.Dispose();
            }
            finally
            {
                try
                {
                    if (Directory.Exists(DataFolder))
                    {
                        Directory.Delete(DataFolder, recursive: true);
                    }
                }
                catch
                {
                    // best-effort cleanup
                }

                _disposed = true;
            }
        }
    }
}
