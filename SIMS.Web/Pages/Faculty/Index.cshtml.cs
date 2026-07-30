using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIMS.Application;
using SIMS.Domain.Entities;
using SIMS.Domain.Interfaces;

namespace SIMS.Web.Pages.Faculty
{
    [Authorize(Roles = "Faculty")]
    public class IndexModel : PageModel
    {
        private readonly CourseService _courseService;
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IUserRepository _userRepository;

        public List<Course> Courses { get; set; } = new List<Course>();
        public Dictionary<string, int> CourseEnrollmentCounts { get; set; } = new Dictionary<string, int>();

        public class CourseDetailVM
        {
            public string CourseId { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public int Credits { get; set; }
            public int MaxCapacity { get; set; }
            public decimal Fee { get; set; }
            public string FacultyId { get; set; } = string.Empty;
            public string FacultyName { get; set; } = "Unassigned";
            public string Department { get; set; } = "N/A";
            public int EnrolledCount { get; set; }
        }

        public List<CourseDetailVM> AllSystemCourses { get; set; } = new List<CourseDetailVM>();

        [BindProperty(SupportsGet = true)]
        public string SelectedCourseId { get; set; } = string.Empty;

        public class StudentEnrollmentDetail
        {
            public string StudentId { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Role { get; set; } = "Student";
            public string Email { get; set; } = string.Empty;
            public DateTime EnrollmentDate { get; set; }
            public string Grade { get; set; } = string.Empty;
        }

        public List<StudentEnrollmentDetail> EnrolledStudents { get; set; } = new List<StudentEnrollmentDetail>();

        [BindProperty]
        public string GradeStudentId { get; set; } = string.Empty;
        
        [BindProperty]
        public string GradeValue { get; set; } = string.Empty;

        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public IndexModel(
            CourseService courseService,
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            IStudentRepository studentRepository,
            IUserRepository userRepository)
        {
            _courseService = courseService;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _studentRepository = studentRepository;
            _userRepository = userRepository;
        }

        public async Task OnGetAsync()
        {
            await LoadCoursesAndStudentsAsync();
        }

        public async Task<IActionResult> OnPostUpdateGradeAsync()
        {
            await LoadCoursesAndStudentsAsync();

            if (string.IsNullOrWhiteSpace(SelectedCourseId) || string.IsNullOrWhiteSpace(GradeStudentId) || string.IsNullOrWhiteSpace(GradeValue))
            {
                ErrorMessage = "Course ID, Student ID, and Grade are required.";
                return Page();
            }

            var isAssigned = Courses.Any(c => string.Equals(c.CourseId, SelectedCourseId, StringComparison.OrdinalIgnoreCase));
            if (!isAssigned)
            {
                ErrorMessage = "You are not authorized to grade students in this course.";
                return Page();
            }

            try
            {
                var success = await _courseService.GradeStudentAsync(GradeStudentId, SelectedCourseId, GradeValue);
                if (success)
                {
                    SuccessMessage = $"Successfully updated grade to {GradeValue} for student ID: {GradeStudentId}.";
                }
                else
                {
                    ErrorMessage = "Failed to update grade.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Validation Error: {ex.Message}";
            }

            await LoadCoursesAndStudentsAsync();
            return Page();
        }

        private async Task LoadCoursesAndStudentsAsync()
        {
            var facultyId = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(facultyId))
            {
                Courses = await _courseService.GetCoursesByFacultyAsync(facultyId);
            }
            else
            {
                Courses = new List<Course>();
            }

            foreach (var course in Courses)
            {
                var enrollments = await _enrollmentRepository.GetByCourseIdAsync(course.CourseId);
                CourseEnrollmentCounts[course.CourseId] = enrollments?.Count ?? 0;
            }

            // Load all system courses & faculty info
            var allCourses = await _courseRepository.GetAllAsync() ?? new List<Course>();
            var allUsers = await _userRepository.GetAllAsync() ?? new List<User>();
            var facultyDict = allUsers.OfType<SIMS.Domain.Entities.Faculty>().ToDictionary(f => f.Id, f => f, StringComparer.OrdinalIgnoreCase);

            AllSystemCourses.Clear();
            foreach (var course in allCourses)
            {
                var enrollments = await _enrollmentRepository.GetByCourseIdAsync(course.CourseId);
                var fName = "Unassigned";
                var dept = "N/A";

                if (!string.IsNullOrWhiteSpace(course.FacultyId) && facultyDict.TryGetValue(course.FacultyId, out var fac))
                {
                    fName = fac.Username;
                    dept = string.IsNullOrWhiteSpace(fac.Department) ? "General" : fac.Department;
                }

                AllSystemCourses.Add(new CourseDetailVM
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Credits = course.Credits,
                    MaxCapacity = course.MaxCapacity,
                    Fee = course.Fee,
                    FacultyId = course.FacultyId,
                    FacultyName = fName,
                    Department = dept,
                    EnrolledCount = enrollments?.Count ?? 0
                });
            }

            if (!string.IsNullOrWhiteSpace(SelectedCourseId))
            {
                var isAssigned = Courses.Any(c => string.Equals(c.CourseId, SelectedCourseId, StringComparison.OrdinalIgnoreCase));
                if (isAssigned)
                {
                    var enrollments = await _enrollmentRepository.GetByCourseIdAsync(SelectedCourseId);
                    if (enrollments != null)
                    {
                        foreach (var enrollment in enrollments)
                        {
                            var student = await _studentRepository.GetByIdAsync(enrollment.StudentId);
                            EnrolledStudents.Add(new StudentEnrollmentDetail
                            {
                                StudentId = enrollment.StudentId,
                                Username = student?.Username ?? "Unknown Student",
                                Role = student?.Role.ToString() ?? "Student",
                                Email = student?.Email ?? string.Empty,
                                EnrollmentDate = enrollment.EnrollmentDate,
                                Grade = enrollment.Grade
                            });
                        }
                    }
                }
                else
                {
                    ErrorMessage = "You are not authorized to view the roster for this course.";
                    SelectedCourseId = string.Empty;
                }
            }
        }
    }
}
