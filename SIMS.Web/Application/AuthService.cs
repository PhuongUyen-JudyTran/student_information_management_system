using System;
using System.Threading.Tasks;
using SIMS.Domain.Entities;
using SIMS.Domain.Interfaces;
using SIMS.Domain.Factories;

namespace SIMS.Application
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly UserFactory _userFactory;

        public AuthService(
            IUserRepository userRepository, 
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            UserFactory userFactory)
        {
            _userRepository = userRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _userFactory = userFactory;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            var success = await _userRepository.DeleteAsync(id);
            if (!success) return false;

            // Cascade delete: clean up student enrollments
            var enrollments = await _enrollmentRepository.GetByStudentIdAsync(id);
            if (enrollments != null)
            {
                foreach (var enrollment in enrollments)
                {
                    await _enrollmentRepository.DeleteAsync(id, enrollment.CourseId);
                }
            }

            // Cascade cleanup: clear faculty assignment from courses
            var courses = await _courseRepository.GetAllAsync();
            if (courses != null)
            {
                foreach (var course in courses.Where(c => string.Equals(c.FacultyId, id, StringComparison.OrdinalIgnoreCase)))
                {
                    course.FacultyId = string.Empty;
                    await _courseRepository.UpdateAsync(course);
                }
            }

            return true;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null) return null;

            var hashedPassword = PasswordHelper.HashPassword(password);
            if (string.Equals(user.PasswordHash, hashedPassword, StringComparison.Ordinal))
            {
                return user;
            }

            return null;
        }

        public async Task<bool> RegisterUserAsync(
            string username, 
            string password, 
            string email, 
            UserRole role, 
            DateTime dateOfBirth = default,
            string? program = null, 
            string? department = null, 
            string? adminLevel = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
                return false;

            var idPrefix = role switch
            {
                UserRole.Student => "STD",
                UserRole.Faculty => "FAC",
                UserRole.Administrator => "ADM",
                _ => "USR"
            };
            var uniqueId = idPrefix + Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();

            var user = _userFactory.CreateUser(role, uniqueId, username, email);
            user.PasswordHash = PasswordHelper.HashPassword(password);

            if (user is Student student)
            {
                student.DateOfBirth = dateOfBirth;
                student.Program = program ?? string.Empty;
            }
            else if (user is Faculty faculty)
            {
                faculty.Department = department ?? string.Empty;
            }
            else if (user is Administrator admin)
            {
                admin.AdminLevel = adminLevel ?? string.Empty;
            }

            user.Validate();

            return await _userRepository.AddAsync(user);
        }
    }
}
