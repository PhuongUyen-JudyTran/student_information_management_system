using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIMS.Application;
using SIMS.Domain.Entities;

namespace SIMS.Web.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly AuthService _authService;

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

        [BindProperty]
        public string Program { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public RegisterModel(AuthService authService)
        {
            _authService = authService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return Page();
            }

            try
            {
                var success = await _authService.RegisterUserAsync(
                    Username,
                    Password,
                    Email,
                    UserRole.Student,
                    DateOfBirth,
                    Program,
                    null,
                    null
                );

                if (success)
                {
                    TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                    return RedirectToPage("/Account/Login");
                }
                else
                {
                    ErrorMessage = "Registration failed. Username may already be taken.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }
}
