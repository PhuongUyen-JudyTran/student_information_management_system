using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SIMS.Domain.Entities;
using SIMS.Infrastructure.Data;
using SIMS.Infrastructure.Locking;
using SIMS.Infrastructure.Repositories;

namespace SIMS.Tests
{
    public class CsvRepositoryTests : IDisposable
    {
        private readonly string _tempFolder;
        private readonly ICsvFileLockManager _lockManager;
        private readonly CsvContext _context;

        public CsvRepositoryTests()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), "SIMS_Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempFolder);

            _lockManager = new CsvFileLockManager();
            _context = new CsvContext(_tempFolder, _lockManager);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempFolder))
            {
                try
                {
                    Directory.Delete(_tempFolder, true);
                }
                catch
                {
                    // Ignore cleanup errors in test teardown
                }
            }
        }

        [Fact]
        public async Task CsvContext_WritesAndReadsCorrectly()
        {
            // Arrange
            var filename = "test_file.csv";
            var lines = new List<string> { "row1,col2", "row2,col2" };

            // Act
            await _context.WriteAllLinesAsync(filename, lines);
            var readLines = await _context.ReadLinesAsync(filename);

            // Assert
            Assert.Equal(lines.Count, readLines.Count);
            Assert.Equal(lines[0], readLines[0]);
            Assert.Equal(lines[1], readLines[1]);
        }

        [Fact]
        public async Task CsvHelper_HandlesDoubleQuotesAndCommas()
        {
            // Arrange
            var originalFields = new List<string> { "STD001", "John \"The Coder\" Doe", "johndoe, jr.", "IT" };

            // Act
            var formattedLine = CsvHelper.FormatLine(originalFields);
            var parsedFields = CsvHelper.ParseLine(formattedLine);

            // Assert
            Assert.Equal(originalFields.Count, parsedFields.Count);
            Assert.Equal(originalFields[0], parsedFields[0]);
            Assert.Equal(originalFields[1], parsedFields[1]);
            Assert.Equal(originalFields[2], parsedFields[2]);
            Assert.Equal(originalFields[3], parsedFields[3]);
        }

        [Fact]
        public async Task CsvContext_WriteConcurrency_IsSafe()
        {
            // Arrange
            var filename = "concurrent.csv";
            var tasks = new List<Task>();
            int writerCount = 10;

            // Act
            for (int w = 0; w < writerCount; w++)
            {
                int writerId = w;
                tasks.Add(Task.Run(async () =>
                {
                    var lines = new List<string> { $"line_{writerId}_1", $"line_{writerId}_2" };
                    await _context.WriteAllLinesAsync(filename, lines);
                }));
            }

            // Await all concurrent tasks; if lock manager fails, it throws IOException
            var exception = await Record.ExceptionAsync(() => Task.WhenAll(tasks));

            // Assert
            Assert.Null(exception);

            var finalLines = await _context.ReadLinesAsync(filename);
            Assert.NotEmpty(finalLines);
            Assert.Equal(2, finalLines.Count); // The last writer wins, file remains in valid state
        }

        [Fact]
        public async Task CsvUserRepository_UpdatesExistingUser_Correctly()
        {
            // Arrange
            var userRepo = new CsvUserRepository(_context);
            var student = new Student
            {
                Id = "STD999",
                Username = "updatecandidate",
                PasswordHash = "hash123",
                Email = "old@uni.edu",
                DateOfBirth = new DateTime(2000, 1, 1),
                Program = "Information Technology"
            };

            await userRepo.AddAsync(student);

            // Act
            student.Email = "new@uni.edu";
            student.Program = "Honors Information Technology";
            var updateResult = await userRepo.UpdateAsync(student);

            // Assert
            Assert.True(updateResult);
            
            var updatedUser = (await userRepo.GetAllAsync()).FirstOrDefault(u => u.Id == "STD999") as Student;
            Assert.NotNull(updatedUser);
            Assert.Equal("new@uni.edu", updatedUser.Email);
            Assert.Equal("Honors Information Technology", updatedUser.Program);
        }
    }
}
