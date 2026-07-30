using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIMS.Domain.Entities;

namespace SIMS.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

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
        }

        return RedirectToPage("/Account/Login");
    }
}
