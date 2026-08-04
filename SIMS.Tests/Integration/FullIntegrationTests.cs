using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SIMS.Application;
using SIMS.Domain.Entities;
using SIMS.Infrastructure.Data;
using SIMS.Infrastructure.Locking;
using Xunit;

namespace SIMS.Tests.Integration
{
    /// <summary>
    /// Production-quality integration tests that validate data flow across layers:
    /// Service -> Repository -> CSV file and back after reload.
    /// Uses CustomWebApplicationFactory which creates an isolated temporary CSV folder per test class instance.
    /// </summary>
    public class FullIntegrationTests : IAsyncLifetime
    {
        private readonly CustomWebApplicationFactory _factory;
        private IServiceScope? _scope;

        public FullIntegrationTests()
        {
            _factory = new CustomWebApplicationFactory();
        }

        public Task InitializeAsync()
        {
            // create a scope we can reuse for tests; some tests create their own scopes when needed
            _scope = _factory.Services.CreateScope();
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _scope?.Dispose();
            _factory.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task AuthService_RegisterUser_PersistsToRepositoryAndCsv()
        {
            // Arrange
            var authService = _scope!.ServiceProvider.GetRequiredService<AuthService>();
            var userRepo = _scope.ServiceProvider.GetRequiredService<Domain.Interfaces.IUserRepository>();
            var username = "int_user_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var password = "Test123!";
            var email = username + "@example.com";

            // Act
            var registered = await authService.RegisterUserAsync(
                username,
                password,
                email,
                UserRole.Student,
                DateTime.Today.AddYears(-20),
                "Information Technology",
                null,
                null
            );

            // Assert
            registered.Should().BeTrue();

            // Verify repository can retrieve the user
            var user = await userRepo.GetByUsernameAsync(username);
            user.Should().NotBeNull();
            user!.Username.Should().Be(username);

            // Verify CSV contains the user record
            var usersCsv = Path.Combine(_factory.DataFolder, "users.csv");
            File.Exists(usersCsv).Should().BeTrue();
            var lines = await File.ReadAllLinesAsync(usersCsv);
            lines.Should().Contain(l => l.Contains(username, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task CourseService_CreateCourse_PersistsToCsv_AndCanBeReloadedFromRepository()
        {
            // Arrange
            var courseService = _scope!.ServiceProvider.GetRequiredService<CourseService>();
            var courseRepo = _scope.ServiceProvider.GetRequiredService<Domain.Interfaces.ICourseRepository>();

            var course = new Course
            {
                CourseId = "CSINT" + Guid.NewGuid().ToString("N").Substring(0, 5),
                Title = "Integration Testing 101",
                Credits = 3,
                MaxCapacity = 50,
                Fee = 1000m,
                FacultyId = string.Empty
            };

            // Act
            var created = await courseService.CreateCourseAsync(course);

            // Assert
            created.Should().BeTrue();

            // Check CSV file
            var coursesCsv = Path.Combine(_factory.DataFolder, "courses.csv");
            File.Exists(coursesCsv).Should().BeTrue();
            var lines = await File.ReadAllLinesAsync(coursesCsv);
            lines.Should().Contain(l => l.Contains(course.CourseId, StringComparison.OrdinalIgnoreCase));

            // Simulate app reload: create a fresh CsvContext that reads the same folder
            var lockManager = _scope.ServiceProvider.GetRequiredService<ICsvFileLockManager>();
            var reloadContext = new CsvContext(_factory.DataFolder, lockManager);
            var reloadLines = await reloadContext.ReadLinesAsync("courses.csv");
            reloadLines.Should().Contain(l => l.Contains(course.CourseId, StringComparison.OrdinalIgnoreCase));

            // Also verify repository read after reload (new scope)
            using var newScope = _factory.Services.CreateScope();
            var repoAfter = newScope.ServiceProvider.GetRequiredService<Domain.Interfaces.ICourseRepository>();
            var loaded = await repoAfter.GetByIdAsync(course.CourseId);
            loaded.Should().NotBeNull();
            loaded!.Title.Should().Be(course.Title);
        }

        [Fact]
        public async Task StudentService_RegisterStudent_AutoEnrollsAndPersistsEnrollments()
        {
            // Arrange
            // create sample courses that match student's program so StandardAssignmentStrategy will pick them
            var courseService = _scope!.ServiceProvider.GetRequiredService<CourseService>();
            var studentService = _scope.ServiceProvider.GetRequiredService<StudentService>();
            var enrollmentRepo = _scope.ServiceProvider.GetRequiredService<Domain.Interfaces.IEnrollmentRepository>();

            var c1 = new Course { CourseId = "CS200", Title = "Algo", Credits = 3, MaxCapacity = 30, Fee = 500, FacultyId = "" };
            var c2 = new Course { CourseId = "CS201", Title = "DS", Credits = 3, MaxCapacity = 30, Fee = 500, FacultyId = "" };
            await courseService.CreateCourseAsync(c1);
            await courseService.CreateCourseAsync(c2);

            var student = new Student
            {
                Id = "SINT" + Guid.NewGuid().ToString("N").Substring(0, 6),
                Username = "stud_" + Guid.NewGuid().ToString("N").Substring(0, 6),
                PasswordHash = PasswordHelper.HashPassword("pass123"),
                Email = "st@int.example",
                DateOfBirth = DateTime.Today.AddYears(-20),
                Program = "Information Technology"
            };

            // Act
            var registered = await studentService.RegisterStudentAsync(student);

            // Assert
            registered.Should().BeTrue();

            // Enrollments should be added by AutoEnrollCoursesAsync; check enrollments CSV
            var enrollmentsCsv = Path.Combine(_factory.DataFolder, "enrollments.csv");
            File.Exists(enrollmentsCsv).Should().BeTrue();
            var lines = await File.ReadAllLinesAsync(enrollmentsCsv);
            // should contain at least one enrollment for our student
            lines.Should().Contain(l => l.Contains(student.Id, StringComparison.OrdinalIgnoreCase));

            // Also ensure repository can return enrollments for this student
            var studentEnrollments = await enrollmentRepo.GetByStudentIdAsync(student.Id);
            studentEnrollments.Should().NotBeNull();
            studentEnrollments!.Any(e => string.Equals(e.StudentId, student.Id, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();

            // Finally, simulate reload using a fresh CsvContext and verify persistence
            var lockManager = _scope.ServiceProvider.GetRequiredService<ICsvFileLockManager>();
            var reloadContext = new CsvContext(_factory.DataFolder, lockManager);
            var reloadEnrollments = await reloadContext.ReadLinesAsync("enrollments.csv");
            reloadEnrollments.Should().Contain(l => l.Contains(student.Id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
