using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceUser.Domain;
using System.Security.Claims;

namespace FarmaView.Pages.Auth
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly IUserService _users;
        public ChangePasswordModel(IUserService users) => _users = users;

        [BindProperty] public string Current { get; set; } = "";
        [BindProperty] public string New1 { get; set; } = "";
        [BindProperty] public string New2 { get; set; } = "";

        public void OnGet() { }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Current))
                ModelState.AddModelError(nameof(Current), "Ingresa tu contraseña actual.");
            if (string.IsNullOrWhiteSpace(New1) || New1.Length < 8)
                ModelState.AddModelError(nameof(New1), "La nueva contraseña debe tener al menos 8 caracteres.");
            if (New1 != New2)
                ModelState.AddModelError(nameof(New2), "Las contraseñas no coinciden.");

            if (!ModelState.IsValid) return Page();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Forbid();

            try
            {
                await _users.ChangePasswordAsync(userId, Current, New1);

                var u = await _users.GetByIdAsync(userId) ?? throw new Exception("Usuario no encontrado.");
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, u.id.ToString()),
                    new Claim(ClaimTypes.Name, u.username),
                    new Claim(ClaimTypes.Email, u.mail ?? ""),
                    new Claim(ClaimTypes.Role, u.role.ToString()),
                    new Claim("HasChangedPassword", u.has_changed_password ? "true" : "false"),
                    new Claim("PwdVer", u.password_version.ToString())
                };

                var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(id));

                TempData["SuccessMessage"] = "Contraseña actualizada correctamente.";
                return RedirectToPage("/Users/Profile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return Page();
            }
        }
    }
}
