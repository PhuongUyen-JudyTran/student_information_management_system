using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using SIMS.Application;
using SIMS.Domain.Entities;
using SIMS.Domain.Interfaces;
using SIMS.Domain.Factories;

namespace SIMS.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IEnrollmentRepository> _mockEnrollmentRepo;
        private readonly Mock<ICourseRepository> _mockCourseRepo;
        private readonly UserFactory _userFactory;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockEnrollmentRepo = new Mock<IEnrollmentRepository>();
            _mockCourseRepo = new Mock<ICourseRepository>();
            _userFactory = new UserFactory();
            _authService = new AuthService(_mockUserRepo.Object, _mockEnrollmentRepo.Object, _mockCourseRepo.Object, _userFactory);
        }

        [Fact]
        public async Task LoginAsync_CorrectCredentials_ReturnsUser()
        {
            // Arrange
            var username = "testuser";
            var rawPassword = "password123";
            var hashedPassword = PasswordHelper.HashPassword(rawPassword);

            var student = new Student
            {
                Id = "STD001",
                Username = username,
                PasswordHash = hashedPassword,
                Email = "test@uni.edu"
            };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(username))
                .ReturnsAsync(student);

            // Act
            var result = await _authService.LoginAsync(username, rawPassword);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            Assert.Equal(UserRole.Student, result.Role);
        }

        [Fact]
        public async Task LoginAsync_IncorrectPassword_ReturnsNull()
        {
            // Arrange
            var username = "testuser";
            var correctPassword = "password123";
            var wrongPassword = "wrongpassword";
            var hashedPassword = PasswordHelper.HashPassword(correctPassword);

            var student = new Student
            {
                Id = "STD001",
                Username = username,
                PasswordHash = hashedPassword,
                Email = "test@uni.edu"
            };

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(username))
                .ReturnsAsync(student);

            // Act
            var result = await _authService.LoginAsync(username, wrongPassword);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterUserAsync_ValidStudent_CallsAddAsyncWithHashedPassword()
        {
            // Arrange
            var username = "newstudent";
            var password = "secretPassword";
            var email = "new@uni.edu";
            var role = UserRole.Student;
            var dob = DateTime.Today.AddYears(-18);
            var program = "Information Technology";

            _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterUserAsync(
                username, password, email, role, dob, program, null, null);

            // Assert
            Assert.True(result);
            _mockUserRepo.Verify(r => r.AddAsync(It.Is<User>(u =>
                u.Username == username &&
                u.Email == email &&
                u.Role == UserRole.Student &&
                u.PasswordHash == PasswordHelper.HashPassword(password) &&
                ((Student)u).Program == program &&
                ((Student)u).DateOfBirth == dob
            )), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ValidStudent_DeletesUserAndEnrollments()
        {
            // Arrange
            string userId = "STD123";
            _mockUserRepo.Setup(r => r.DeleteAsync(userId)).ReturnsAsync(true);

            var enrollments = new List<Enrollment>
            {
                new Enrollment { StudentId = userId, CourseId = "CS101" },
                new Enrollment { StudentId = userId, CourseId = "CS102" }
            };
            _mockEnrollmentRepo.Setup(r => r.GetByStudentIdAsync(userId)).ReturnsAsync(enrollments);
            _mockEnrollmentRepo.Setup(r => r.DeleteAsync(userId, It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _authService.DeleteUserAsync(userId);

            // Assert
            Assert.True(result);
            _mockUserRepo.Verify(r => r.DeleteAsync(userId), Times.Once);
            _mockEnrollmentRepo.Verify(r => r.DeleteAsync(userId, "CS101"), Times.Once);
            _mockEnrollmentRepo.Verify(r => r.DeleteAsync(userId, "CS102"), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_NonExistentUser_ReturnsFalse()
        {
            // Arrange
            string userId = "NONE";
            _mockUserRepo.Setup(r => r.DeleteAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _authService.DeleteUserAsync(userId);

            // Assert
            Assert.False(result);
            _mockUserRepo.Verify(r => r.DeleteAsync(userId), Times.Once);
            _mockEnrollmentRepo.Verify(r => r.GetByStudentIdAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
