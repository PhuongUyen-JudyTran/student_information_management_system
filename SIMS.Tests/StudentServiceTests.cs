using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using SIMS.Application;
using SIMS.Domain.Entities;
using SIMS.Domain.Interfaces;

namespace SIMS.Tests
{
    public class StudentServiceTests
    {
        private readonly Mock<IStudentRepository> _studentRepoMock;
        private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock;
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly List<ICourseAssignmentStrategy> _strategies;
        private readonly StudentService _studentService;

        public StudentServiceTests()
        {
            _studentRepoMock = new Mock<IStudentRepository>();
            _enrollmentRepoMock = new Mock<IEnrollmentRepository>();
            _courseRepoMock = new Mock<ICourseRepository>();
            _strategies = new List<ICourseAssignmentStrategy>();

            _studentService = new StudentService(
                _studentRepoMock.Object,
                _enrollmentRepoMock.Object,
                _courseRepoMock.Object,
                _strategies
            );
        }

        [Fact]
        public async Task EnrollInCourseAsync_ValidStudentAndCourse_ReturnsTrue()
        {
            // Arrange
            string studentId = "STD123";
            string courseId = "SE08101";

            var student = new Student
            {
                Id = studentId,
                Username = "johndoe",
                Email = "johndoe@uni.edu",
                DateOfBirth = DateTime.Today.AddYears(-20),
                Program = "Information Technology"
            };

            var course = new Course
            {
                CourseId = courseId,
                Title = "Intro Docker",
                Credits = 3,
                MaxCapacity = 30
            };

            _studentRepoMock.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(student);
            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);
            _enrollmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Enrollment>())).ReturnsAsync(true);

            // Act
            var result = await _studentService.EnrollInCourseAsync(studentId, courseId);

            // Assert
            Assert.True(result);
            _enrollmentRepoMock.Verify(r => r.AddAsync(It.Is<Enrollment>(e => 
                e.StudentId == studentId && 
                e.CourseId == courseId && 
                e.Grade == "Pending"
            )), Times.Once);
        }

        [Fact]
        public async Task EnrollInCourseAsync_NonExistentStudent_ReturnsFalse()
        {
            // Arrange
            string studentId = "STD_NONE";
            string courseId = "SE08101";

            _studentRepoMock.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync((Student?)null);

            // Act
            var result = await _studentService.EnrollInCourseAsync(studentId, courseId);

            // Assert
            Assert.False(result);
            _enrollmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Enrollment>()), Times.Never);
        }

        [Fact]
        public async Task EnrollInCourseAsync_NonExistentCourse_ReturnsFalse()
        {
            // Arrange
            string studentId = "STD123";
            string courseId = "SE_NONE";

            var student = new Student
            {
                Id = studentId,
                Username = "johndoe",
                Email = "johndoe@uni.edu",
                DateOfBirth = DateTime.Today.AddYears(-20),
                Program = "Information Technology"
            };

            _studentRepoMock.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(student);
            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync((Course?)null);

            // Act
            var result = await _studentService.EnrollInCourseAsync(studentId, courseId);

            // Assert
            Assert.False(result);
            _enrollmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Enrollment>()), Times.Never);
        }
    }
}
