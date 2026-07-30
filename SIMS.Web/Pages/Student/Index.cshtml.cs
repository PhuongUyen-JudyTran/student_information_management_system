using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIMS.Application;
using SIMS.Domain.Entities;
using SIMS.Domain.Interfaces;

namespace SIMS.Web.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class IndexModel : PageModel
    {
        private readonly StudentService _studentService;
        private readonly ICourseRepository _courseRepository;

        private readonly IEnumerable<ICourseAssignmentStrategy> _strategies;

        public SIMS.Domain.Entities.Student? StudentProfile { get; set; }
        
        public class EnrolledCourseDetail
        {
            public string CourseId { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public int Credits { get; set; }
            public DateTime EnrollmentDate { get; set; }
            public string Grade { get; set; } = string.Empty;
        }

        public List<EnrolledCourseDetail> EnrolledCourses { get; set; } = new List<EnrolledCourseDetail>();
        public List<Course> AvailableCourses { get; set; } = new List<Course>();
        public List<Course> RecommendedCourses { get; set; } = new List<Course>();
        public HashSet<string> PaidCourseIds { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public string ActiveStrategyName { get; set; } = "Standard Academic Assignment Strategy";

        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public IndexModel(
            StudentService studentService, 
            ICourseRepository courseRepository,
            IEnumerable<ICourseAssignmentStrategy> strategies)
        {
            _studentService = studentService;
            _courseRepository = courseRepository;
            _strategies = strategies;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            SuccessMessage = TempData["SuccessMessage"]?.ToString() ?? string.Empty;
            ErrorMessage = TempData["ErrorMessage"]?.ToString() ?? string.Empty;

            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "User ID claim not found.";
                return Page();
            }

            StudentProfile = await _studentService.GetStudentProfileAsync(userId);
            if (StudentProfile == null)
            {
                ErrorMessage = "Student profile not found.";
                return Page();
            }

            foreach (var enrollment in StudentProfile.Enrollments)
            {
                if (enrollment.IsPaid)
                {
                    PaidCourseIds.Add(enrollment.CourseId);
                }

                var course = await _courseRepository.GetByIdAsync(enrollment.CourseId);
                EnrolledCourses.Add(new EnrolledCourseDetail
                {
                    CourseId = enrollment.CourseId,
                    Title = course?.Title ?? "Unknown Course",
                    Credits = course?.Credits ?? 0,
                    EnrollmentDate = enrollment.EnrollmentDate,
                    Grade = enrollment.Grade
                });
            }

            AvailableCourses = await _studentService.GetAvailableCoursesForStudentAsync(userId);

            var strategy = _strategies.FirstOrDefault(s => s.CanApply(StudentProfile));
            if (strategy != null)
            {
                ActiveStrategyName = strategy.GetType().Name == "HonorsAssignmentStrategy"
                    ? "Honors Academic Strategy (High GPA / Honors Program)"
                    : "Standard Program Alignment Strategy";

                var recEnrollments = strategy.AssignCourses(StudentProfile, AvailableCourses);
                var recIds = recEnrollments.Select(e => e.CourseId).ToHashSet(StringComparer.OrdinalIgnoreCase);
                RecommendedCourses = AvailableCourses.Where(c => recIds.Contains(c.CourseId)).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmPaymentAsync([FromForm] string confirmCourseIds)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User ID claim not found.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(confirmCourseIds))
            {
                TempData["ErrorMessage"] = "No courses selected for payment confirmation.";
                return RedirectToPage();
            }

            var courseIds = confirmCourseIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            try
            {
                var success = await _studentService.ConfirmPaymentAsync(userId, courseIds);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Tuition payment confirmed for {courseIds.Count} course(s)! You can now click Enroll.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to process payment confirmation.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Payment confirmation failed: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEnrollAsync(string courseId)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User ID claim not found.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(courseId))
            {
                TempData["ErrorMessage"] = "Course ID is required.";
                return RedirectToPage();
            }

            try
            {
                var success = await _studentService.EnrollInCourseAsync(userId, courseId);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Successfully enrolled in course {courseId}!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to enroll. You might already be enrolled in this course.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Enrollment failed: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEnrollMultipleAsync([FromForm] string selectedCourseIds)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User ID claim not found.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(selectedCourseIds))
            {
                TempData["ErrorMessage"] = "No courses were selected for enrollment.";
                return RedirectToPage();
            }

            var courseIds = selectedCourseIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            if (!courseIds.Any())
            {
                TempData["ErrorMessage"] = "No valid courses selected.";
                return RedirectToPage();
            }

            try
            {
                var success = await _studentService.EnrollInCoursesAsync(userId, courseIds);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Successfully enrolled in {courseIds.Count} selected course(s)!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to enroll in selected courses. You may already be enrolled in them.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Enrollment failed: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
