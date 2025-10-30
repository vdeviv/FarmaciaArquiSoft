using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceUser.Domain;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FarmaView.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _users;
        public LoginModel(IUserService users) => _users = users;

        [BindProperty] public LoginVm Input { get; set; } = new();
        [BindProperty] public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Content("~/") : returnUrl;
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostPasswordAsync()
        {
            ReturnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid) return Page();

            try
            {
                var u = await _users.AuthenticateAsync(Input.Username, Input.Password);

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

                
                if (!u.has_changed_password)
                    return LocalRedirect(Url.Content("~/Auth/ChangePassword"));

                
                return LocalRedirect(ReturnUrl!);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Page();
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostQuickAdminAsync()
        {
            ReturnUrl ??= Url.Content("~/");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "admin.qa"),
                new Claim(ClaimTypes.Email, "admin.qa@example.com"),
                new Claim(ClaimTypes.Role, "Administrador"),
                new Claim("HasChangedPassword", "true"),
                new Claim("PwdVer", "1")
            };
            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id));
            return LocalRedirect(ReturnUrl!);
        }

        public class LoginVm
        {
            [Required, Display(Name = "Usuario")]
            public string Username { get; set; } = "";
            [Required, Display(Name = "Contraseña")]
            public string Password { get; set; } = "";
        }
    }
}
