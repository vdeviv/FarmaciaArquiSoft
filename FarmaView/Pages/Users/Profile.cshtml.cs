using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceUser.Domain;
using System.Security.Claims;

namespace FarmaView.Pages.Users
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IUserService _users;
        public ProfileModel(IUserService users) => _users = users;

        public User? Current { get; private set; }

        public async Task<IActionResult> OnGet()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId)) return Forbid();

            Current = await _users.GetByIdAsync(userId);
            if (Current is null) return NotFound();

            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId)) return Forbid();

            await _users.SoftDeleteAsync(userId, userId);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "Tu cuenta fue desactivada. Vuelve cuando la resucites con un admin.";
            return RedirectToPage("/Auth/Login");
        }
    }
}
