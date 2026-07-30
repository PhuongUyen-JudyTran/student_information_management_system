using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIMS.Application;
using SIMS.Domain.Entities;
using SIMS.Domain.Interfaces;

namespace SIMS.Web.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly AuthService _authService;
        private readonly CourseService _courseService;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;

        [BindProperty]
        public string RegUsername { get; set; } = string.Empty;
        [BindProperty]
        public string RegPassword { get; set; } = string.Empty;
        [BindProperty]
        public string RegEmail { get; set; } = string.Empty;
        [BindProperty]
        public string RegRole { get; set; } = string.Empty;
        [BindProperty]
        public DateTime RegDOB { get; set; } = DateTime.Today.AddYears(-18);
        [BindProperty]
        public string RegProgram { get; set; } = string.Empty;
        [BindProperty]
        public string RegDepartment { get; set; } = string.Empty;
        [BindProperty]
        public string RegAdminLevel { get; set; } = string.Empty;

        [BindProperty]
        public string CourseId { get; set; } = string.Empty;
        [BindProperty]
        public string CourseTitle { get; set; } = string.Empty;
        [BindProperty]
        public int CourseCredits { get; set; } = 3;
        [BindProperty]
        public int CourseCapacity { get; set; } = 30;
        [BindProperty]
        public decimal CourseFee { get; set; } = 1500000m;
        [BindProperty]
        public string CourseFacultyId { get; set; } = string.Empty;

        [BindProperty]
        public string EditUserId { get; set; } = string.Empty;
        [BindProperty]
        public string EditEmail { get; set; } = string.Empty;
        [BindProperty]
        public DateTime EditDOB { get; set; } = DateTime.Today.AddYears(-18);
        [BindProperty]
        public string EditProgram { get; set; } = string.Empty;
        [BindProperty]
        public string EditDepartment { get; set; } = string.Empty;
        [BindProperty]
        public string EditAdminLevel { get; set; } = string.Empty;

        [BindProperty]
        public string EditCourseId { get; set; } = string.Empty;
        [BindProperty]
        public string EditCourseTitle { get; set; } = string.Empty;
        [BindProperty]
        public int EditCourseCredits { get; set; } = 3;
        [BindProperty]
        public int EditCourseCapacity { get; set; } = 30;
        [BindProperty]
        public decimal EditCourseFee { get; set; } = 1500000m;
        [BindProperty]
        public string EditCourseFacultyId { get; set; } = string.Empty;

        [BindProperty]
        public string AssignCourseId { get; set; } = string.Empty;
        [BindProperty]
        public string AssignFacultyId { get; set; } = string.Empty;

        public List<User> SystemUsers { get; set; } = new List<User>();
        public List<SIMS.Domain.Entities.Student> Students { get; set; } = new List<SIMS.Domain.Entities.Student>();
        public List<SIMS.Domain.Entities.Faculty> FacultyMembers { get; set; } = new List<SIMS.Domain.Entities.Faculty>();
        public List<Administrator> Admins { get; set; } = new List<Administrator>();
        public List<Course> Courses { get; set; } = new List<Course>();

        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public IndexModel(
            AuthService authService,
            CourseService courseService,
            ICourseRepository courseRepository,
            IUserRepository userRepository)
        {
            _authService = authService;
            _courseService = courseService;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostRegisterUserAsync()
        {
            try
            {
                if (!Enum.TryParse<UserRole>(RegRole, out var role))
                {
                    ErrorMessage = "Invalid role selected.";
                    await LoadDataAsync();
                    return Page();
                }

                var dob = role == UserRole.Student ? RegDOB : default;
                var prog = role == UserRole.Student ? RegProgram : null;
                var dept = role == UserRole.Faculty ? RegDepartment : null;
                var level = role == UserRole.Administrator ? RegAdminLevel : null;

                var success = await _authService.RegisterUserAsync(RegUsername, RegPassword, RegEmail, role, dob, prog, dept, level);
                if (success)
                {
                    SuccessMessage = $"Successfully registered user: {RegUsername}";
                    RegUsername = "";
                    RegPassword = "";
                    RegEmail = "";
                }
                else
                {
                    ErrorMessage = "Failed to register user. Username might already exist.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Validation Error: {ex.Message}";
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateCourseAsync()
        {
            try
            {
                var newCourse = new Course
                {
                    CourseId = CourseId,
                    Title = CourseTitle,
                    Credits = CourseCredits,
                    MaxCapacity = CourseCapacity,
                    FacultyId = CourseFacultyId ?? string.Empty,
                    Fee = CourseFee > 0 ? CourseFee : CourseCredits * 500000m
                };

                var success = await _courseService.CreateCourseAsync(newCourse);
                if (success)
                {
                    SuccessMessage = $"Successfully created course: {CourseTitle}";
                    CourseId = "";
                    CourseTitle = "";
                }
                else
                {
                    ErrorMessage = "Failed to create course. Course ID might already exist.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Validation Error: {ex.Message}";
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAssignFacultyAsync()
        {
            try
            {
                var success = await _courseService.AssignFacultyToCourseAsync(AssignCourseId, AssignFacultyId);
                if (success)
                {
                    SuccessMessage = $"Successfully assigned faculty '{AssignFacultyId}' to course '{AssignCourseId}'.";
                }
                else
                {
                    ErrorMessage = "Failed to assign faculty to course.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Assignment Error: {ex.Message}";
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEditUserAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditUserId))
                {
                    ErrorMessage = "User ID is required for editing.";
                    await LoadDataAsync();
                    return Page();
                }

                var allUsers = await _userRepository.GetAllAsync();
                var user = allUsers.FirstOrDefault(u => u.Id == EditUserId);
                if (user == null)
                {
                    ErrorMessage = "User not found.";
                    await LoadDataAsync();
                    return Page();
                }

                user.Email = EditEmail;

                if (user is SIMS.Domain.Entities.Student student)
                {
                    student.DateOfBirth = EditDOB;
                    student.Program = EditProgram;
                }
                else if (user is SIMS.Domain.Entities.Faculty faculty)
                {
                    faculty.Department = EditDepartment;
                }
                else if (user is Administrator admin)
                {
                    admin.AdminLevel = EditAdminLevel;
                }

                var success = await _userRepository.UpdateAsync(user);
                if (success)
                {
                    SuccessMessage = $"Successfully updated user: {user.Username}";
                }
                else
                {
                    ErrorMessage = "Failed to update user.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Update Error: {ex.Message}";
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEditCourseAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditCourseId))
                {
                    ErrorMessage = "Course ID is required for editing.";
                    await LoadDataAsync();
                    return Page();
                }

                var course = await _courseRepository.GetByIdAsync(EditCourseId);
                if (course == null)
                {
                    ErrorMessage = "Course not found.";
                    await LoadDataAsync();
                    return Page();
                }

                course.Title = EditCourseTitle;
                course.Credits = EditCourseCredits;
                course.MaxCapacity = EditCourseCapacity;
                course.FacultyId = EditCourseFacultyId ?? string.Empty;
                course.Fee = EditCourseFee;

                var success = await _courseService.UpdateCourseAsync(course);
                if (success)
                {
                    SuccessMessage = $"Successfully updated course: {course.Title}";
                }
                else
                {
                    ErrorMessage = "Failed to update course.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Update Error: {ex.Message}";
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
        {
            var loggedInUserId = User.FindFirst("UserId")?.Value;
            if (string.Equals(loggedInUserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "You cannot delete your own administrator account.";
                await LoadDataAsync();
                return Page();
            }

            try
            {
                var success = await _authService.DeleteUserAsync(userId);
                if (success)
                {
                    SuccessMessage = $"Successfully deleted user with ID: {userId}";
                }
                else
                {
                    ErrorMessage = "Failed to delete user.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Delete Error: {ex.Message}";
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteCourseAsync(string courseId)
        {
            try
            {
                var success = await _courseService.DeleteCourseAsync(courseId);
                if (success)
                {
                    SuccessMessage = $"Successfully deleted course with ID: {courseId}";
                }
                else
                {
                    ErrorMessage = "Failed to delete course.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Delete Error: {ex.Message}";
            }

            await LoadDataAsync();
            return Page();
        }

        private async Task LoadDataAsync()
        {
            SystemUsers = await _userRepository.GetAllAsync() ?? new List<User>();
            Students = SystemUsers.OfType<SIMS.Domain.Entities.Student>().ToList();
            FacultyMembers = SystemUsers.OfType<SIMS.Domain.Entities.Faculty>().ToList();
            Admins = SystemUsers.OfType<Administrator>().ToList();
            Courses = await _courseRepository.GetAllAsync() ?? new List<Course>();
        }
    }
}
