using System;
using System.Collections.Generic;
using Xunit;
using SIMS.Application;
using SIMS.Domain.Entities;

namespace SIMS.Tests
{
    public class StrategyTests
    {
        private readonly List<Course> _availableCourses;

        public StrategyTests()
        {
            _availableCourses = new List<Course>
            {
                new Course { CourseId = "CS101", Title = "Intro to IT", Credits = 3, MaxCapacity = 30 },
                new Course { CourseId = "CS102", Title = "Intro to OOP", Credits = 3, MaxCapacity = 30 },
                new Course { CourseId = "CS301", Title = "Software Architecture", Credits = 4, MaxCapacity = 20 },
                new Course { CourseId = "CS401", Title = "Honors Thesis", Credits = 4, MaxCapacity = 10 },
                new Course { CourseId = "BUS101", Title = "Intro to Business", Credits = 3, MaxCapacity = 40 }
            };
        }

        [Fact]
        public void StandardStrategy_CanApply_And_AssignsCorrectCourses()
        {
            // Arrange
            var strategy = new StandardAssignmentStrategy();
            var student = new Student
            {
                Id = "STD001",
                Program = "Information Technology"
            };

            // Act
            var canApply = strategy.CanApply(student);
            var enrollments = strategy.AssignCourses(student, _availableCourses);

            // Assert
            Assert.True(canApply);
            Assert.NotEmpty(enrollments);
            Assert.True(enrollments.Count <= 3);
            
            foreach (var e in enrollments)
            {
                Assert.StartsWith("CS", e.CourseId, StringComparison.OrdinalIgnoreCase);
                Assert.Equal("Pending", e.Grade);
            }
        }

        [Fact]
        public void HonorsStrategy_CanApply_ForHonorsProgram_And_AssignsAdvancedCourses()
        {
            // Arrange
            var strategy = new HonorsAssignmentStrategy();
            
            var honorsStudent = new Student
            {
                Id = "STD002",
                Program = "Honors Information Technology"
            };

            // Act & Assert
            Assert.True(strategy.CanApply(honorsStudent));
            var enrollments = strategy.AssignCourses(honorsStudent, _availableCourses);
            
            Assert.NotEmpty(enrollments);
            Assert.True(enrollments.Count <= 4);
            
            var enrolledIds = enrollments.ConvertAll(e => e.CourseId);
            Assert.Contains("CS301", enrolledIds);
            Assert.Contains("CS401", enrolledIds);
        }

        [Fact]
        public void HonorsStrategy_CanApply_ForHighGPAStudent_And_AssignsCorrectly()
        {
            // Arrange
            var strategy = new HonorsAssignmentStrategy();
            
            var highGpaStudent = new Student
            {
                Id = "STD003",
                Program = "Information Technology",
                Record = new AcademicRecord { CumulativeGPA = 3.8 }
            };

            // Act & Assert
            Assert.True(strategy.CanApply(highGpaStudent));
            var enrollments = strategy.AssignCourses(highGpaStudent, _availableCourses);
            
            Assert.NotEmpty(enrollments);
            Assert.True(enrollments.Count <= 4);
        }
    }
}
