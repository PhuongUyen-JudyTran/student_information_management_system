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
    public class CourseServiceTests
    {
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly Mock<IEnrollmentRepository> _enrollmentRepoMock;
        private readonly Mock<IStudentRepository> _studentRepoMock;
        private readonly CourseService _courseService;

        public CourseServiceTests()
        {
            _courseRepoMock = new Mock<ICourseRepository>();
            _enrollmentRepoMock = new Mock<IEnrollmentRepository>();
            _studentRepoMock = new Mock<IStudentRepository>();

            _courseService = new CourseService(
                _courseRepoMock.Object,
                _enrollmentRepoMock.Object,
                _studentRepoMock.Object
            );
        }

        [Fact]
        public async Task DeleteCourseAsync_ValidCourse_DeletesCourseAndEnrollments()
        {
            // Arrange
            string courseId = "SE08101";
            _courseRepoMock.Setup(r => r.DeleteAsync(courseId)).ReturnsAsync(true);

            var enrollments = new List<Enrollment>
            {
                new Enrollment { StudentId = "STD1", CourseId = courseId },
                new Enrollment { StudentId = "STD2", CourseId = courseId }
            };
            _enrollmentRepoMock.Setup(r => r.GetByCourseIdAsync(courseId)).ReturnsAsync(enrollments);
            _enrollmentRepoMock.Setup(r => r.DeleteAsync(It.IsAny<string>(), courseId)).ReturnsAsync(true);

            // Act
            var result = await _courseService.DeleteCourseAsync(courseId);

            // Assert
            Assert.True(result);
            _courseRepoMock.Verify(r => r.DeleteAsync(courseId), Times.Once);
            _enrollmentRepoMock.Verify(r => r.DeleteAsync("STD1", courseId), Times.Once);
            _enrollmentRepoMock.Verify(r => r.DeleteAsync("STD2", courseId), Times.Once);
        }

        [Fact]
        public async Task DeleteCourseAsync_NonExistentCourse_ReturnsFalse()
        {
            // Arrange
            string courseId = "NONE";
            _courseRepoMock.Setup(r => r.DeleteAsync(courseId)).ReturnsAsync(false);

            // Act
            var result = await _courseService.DeleteCourseAsync(courseId);

            // Assert
            Assert.False(result);
            _courseRepoMock.Verify(r => r.DeleteAsync(courseId), Times.Once);
            _enrollmentRepoMock.Verify(r => r.GetByCourseIdAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
