using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SIMS.Domain.Entities;
using SIMS.Domain.Interfaces;

namespace SIMS.Application
{
    public class StudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IEnumerable<ICourseAssignmentStrategy> _strategies;

        public StudentService(
            IStudentRepository studentRepository, 
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IEnumerable<ICourseAssignmentStrategy> strategies)
        {
            _studentRepository = studentRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _strategies = strategies;
        }

        public async Task<Student?> GetStudentProfileAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId)) return null;

            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null) return null;

            var enrollments = await _enrollmentRepository.GetByStudentIdAsync(studentId);
            student.Enrollments = enrollments ?? new List<Enrollment>();

            double totalGradePoints = 0;
            int totalGradedCredits = 0;
            int totalCreditsEarned = 0;

            foreach (var enrollment in student.Enrollments)
            {
                var course = await _courseRepository.GetByIdAsync(enrollment.CourseId);
                if (course != null)
                {
                    double gradePoint = ConvertGradeToPoint(enrollment.Grade);
                    if (gradePoint >= 0)
                    {
                        totalGradePoints += gradePoint * course.Credits;
                        totalGradedCredits += course.Credits;

                        if (gradePoint > 0)
                        {
                            totalCreditsEarned += course.Credits;
                        }
                    }
                }
            }

            student.Record = new AcademicRecord
            {
                CumulativeGPA = totalGradedCredits > 0 ? Math.Round(totalGradePoints / totalGradedCredits, 2) : 0.0,
                TotalCreditsEarned = totalCreditsEarned
            };

            return student;
        }

        public async Task<bool> RegisterStudentAsync(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));

            student.Validate();

            var success = await _studentRepository.AddAsync(student);
            if (!success) return false;

            await AutoEnrollCoursesAsync(student.Id);

            return true;
        }

        public async Task<bool> EnrollInCourseAsync(string studentId, string courseId)
        {
            if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(courseId)) return false;

            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null) return false;

            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return false;

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

        public async Task<bool> AutoEnrollCoursesAsync(string studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null) return false;

            var fullStudent = await GetStudentProfileAsync(studentId);
            if (fullStudent == null) return false;

            var strategy = _strategies.FirstOrDefault(s => s.CanApply(fullStudent));
            if (strategy == null) return false;

            var availableCourses = await _courseRepository.GetAllAsync();
            var recommendedEnrollments = strategy.AssignCourses(fullStudent, availableCourses);

            bool enrolledAny = false;
            foreach (var enrollment in recommendedEnrollments)
            {
                enrollment.Validate();
                var added = await _enrollmentRepository.AddAsync(enrollment);
                if (added) enrolledAny = true;
            }

            return enrolledAny;
        }

        public async Task<List<Course>> GetAvailableCoursesForStudentAsync(string studentId)
        {
            var allCourses = await _courseRepository.GetAllAsync();
            var enrollments = await _enrollmentRepository.GetByStudentIdAsync(studentId) ?? new List<Enrollment>();
            var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return allCourses.Where(c => !enrolledCourseIds.Contains(c.CourseId)).ToList();
        }

        public async Task<bool> EnrollInCoursesAsync(string studentId, List<string> courseIds)
        {
            if (string.IsNullOrWhiteSpace(studentId) || courseIds == null || !courseIds.Any()) return false;

            bool anySuccess = false;
            foreach (var courseId in courseIds)
            {
                var success = await EnrollInCourseAsync(studentId, courseId);
                if (success) anySuccess = true;
            }
            return anySuccess;
        }

        private double ConvertGradeToPoint(string grade)
        {
            return grade?.ToUpper() switch
            {
                "A" => 4.0,
                "B" => 3.0,
                "C" => 2.0,
                "D" => 1.0,
                "F" => 0.0,
                _ => -1.0
            };
        }
    }
}
