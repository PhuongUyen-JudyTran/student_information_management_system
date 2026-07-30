using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SIMS.Domain.Entities;
using SIMS.Domain.Exceptions;
using SIMS.Domain.Interfaces;

namespace SIMS.Application
{
    public class CourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IStudentRepository _studentRepository;

        public CourseService(
            ICourseRepository courseRepository, 
            IEnrollmentRepository enrollmentRepository,
            IStudentRepository studentRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _studentRepository = studentRepository;
        }

        public async Task<bool> CreateCourseAsync(Course course)
        {
            if (course == null) throw new ArgumentNullException(nameof(course));

            course.Validate();
            return await _courseRepository.AddAsync(course);
        }

        public async Task<bool> UpdateCourseAsync(Course course)
        {
            if (course == null) throw new ArgumentNullException(nameof(course));

            course.Validate();
            return await _courseRepository.UpdateAsync(course);
        }

        public async Task<bool> EnrollStudentInCourseAsync(string studentId, string courseId)
        {
            if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(courseId))
                return false;

            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                throw new ValidationException($"Student with ID {studentId} not found.");

            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new ValidationException($"Course with ID {courseId} not found.");

            var enrollments = await _enrollmentRepository.GetByCourseIdAsync(courseId);
            if (enrollments != null && enrollments.Count >= course.MaxCapacity)
            {
                throw new ValidationException($"Course {course.Title} has reached its maximum capacity of {course.MaxCapacity}.");
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now,
                Grade = "Pending"
            };

            enrollment.Validate();
            return await _enrollmentRepository.AddAsync(enrollment);
        }

        public async Task<bool> UnenrollStudentFromCourseAsync(string studentId, string courseId)
        {
            if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(courseId))
                return false;

            return await _enrollmentRepository.DeleteAsync(studentId, courseId);
        }

        public async Task<bool> GradeStudentAsync(string studentId, string courseId, string grade)
        {
            if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(courseId) || string.IsNullOrWhiteSpace(grade))
                return false;

            var upperGrade = grade.ToUpperInvariant();
            if (upperGrade != "A" && upperGrade != "B" && upperGrade != "C" && upperGrade != "D" && upperGrade != "F")
            {
                throw new ValidationException("Invalid grade value. Valid grades are A, B, C, D, F.");
            }

            var studentEnrollments = await _enrollmentRepository.GetByStudentIdAsync(studentId);
            var existing = studentEnrollments?.FirstOrDefault(e => string.Equals(e.CourseId, courseId, StringComparison.OrdinalIgnoreCase));
            
            if (existing == null)
                throw new ValidationException($"Student is not enrolled in course {courseId}.");

            var deleted = await _enrollmentRepository.DeleteAsync(studentId, courseId);
            if (!deleted) return false;

            var newEnrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = existing.EnrollmentDate,
                Grade = upperGrade
            };

            newEnrollment.Validate();
            return await _enrollmentRepository.AddAsync(newEnrollment);
        }

        public async Task<bool> DeleteCourseAsync(string courseId)
        {
            if (string.IsNullOrWhiteSpace(courseId)) return false;

            var success = await _courseRepository.DeleteAsync(courseId);
            if (!success) return false;

            // Cascade delete: clean up course enrollments
            var enrollments = await _enrollmentRepository.GetByCourseIdAsync(courseId);
            if (enrollments != null)
            {
                foreach (var enrollment in enrollments)
                {
                    await _enrollmentRepository.DeleteAsync(enrollment.StudentId, courseId);
                }
            }

            return true;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _courseRepository.GetAllAsync();
        }

        public async Task<List<Course>> GetCoursesByFacultyAsync(string facultyId)
        {
            if (string.IsNullOrWhiteSpace(facultyId)) return new List<Course>();

            var allCourses = await _courseRepository.GetAllAsync();
            return allCourses.Where(c => string.Equals(c.FacultyId, facultyId, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<bool> AssignFacultyToCourseAsync(string courseId, string facultyId)
        {
            if (string.IsNullOrWhiteSpace(courseId)) return false;

            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new ValidationException($"Course with ID '{courseId}' not found.");

            course.FacultyId = facultyId ?? string.Empty;
            course.Validate();
            return await _courseRepository.UpdateAsync(course);
        }
    }
}
