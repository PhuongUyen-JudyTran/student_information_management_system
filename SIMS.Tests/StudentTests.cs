using System;
using Xunit;
using SIMS.Domain.Entities;
using SIMS.Domain.Exceptions;

namespace SIMS.Tests
{
    public class StudentTests
    {
        [Fact]
        public void Validate_ValidStudent_ShouldPass()
        {
            // Arrange
            var student = new Student
            {
                Id = "STD001",
                Username = "johndoe",
                PasswordHash = "hashedpassword",
                Email = "johndoe@uni.edu",
                DateOfBirth = DateTime.Today.AddYears(-18),
                Program = "Information Technology"
            };

            // Act & Assert (No exception should be thrown)
            student.Validate();
        }

        [Fact]
        public void Validate_StudentUnder16_ShouldThrowValidationException()
        {
            // Arrange
            var student = new Student
            {
                Id = "STD002",
                Username = "kiddoe",
                PasswordHash = "hashedpassword",
                Email = "kid@uni.edu",
                DateOfBirth = DateTime.Today.AddYears(-15), // 15 years old
                Program = "Information Technology"
            };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => student.Validate());
            Assert.Contains("Student must be at least 16 years old", ex.Message);
        }

        [Fact]
        public void Validate_InvalidEmail_ShouldThrowValidationException()
        {
            // Arrange
            var student = new Student
            {
                Id = "STD003",
                Username = "bademail",
                PasswordHash = "hashedpassword",
                Email = "bademail_at_uni.edu", // Invalid format
                DateOfBirth = DateTime.Today.AddYears(-20),
                Program = "Information Technology"
            };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => student.Validate());
            Assert.Contains("Invalid email format", ex.Message);
        }

        [Fact]
        public void Validate_MissingUsername_ShouldThrowValidationException()
        {
            // Arrange
            var student = new Student
            {
                Id = "STD004",
                Username = "", // Missing
                PasswordHash = "hashedpassword",
                Email = "valid@uni.edu",
                DateOfBirth = DateTime.Today.AddYears(-20),
                Program = "Information Technology"
            };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => student.Validate());
            Assert.Contains("Username is required", ex.Message);
        }
    }
}
