using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIMS.Application;
using SIMS.Domain.Entities;

namespace SIMS.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly AuthService _authService;

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public LoginModel(AuthService authService)
        {
            _authService = authService;
        }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
                return RedirectToDashboard(role);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and Password are required.";
                return Page();
            }

            var user = await _authService.LoginAsync(Username, Password);
            if (user == null)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("UserId", user.Id)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToDashboard(user.Role.ToString());
        }

        private IActionResult RedirectToDashboard(string role)
        {
            if (role == UserRole.Administrator.ToString())
            {
                return RedirectToPage("/Admin/Index");
            }
            else if (role == UserRole.Faculty.ToString())
            {
                return RedirectToPage("/Faculty/Index");
            }
            else if (role == UserRole.Student.ToString())
            {
                return RedirectToPage("/Student/Index");
            }

            return RedirectToPage("/Index");
        }
    }
}
