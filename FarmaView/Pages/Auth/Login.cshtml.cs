using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceUser.Domain;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Farmacia_Arqui_Soft.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _users;

        public LoginModel(IUserService users)
        {
            _users = users;
        }

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
                    new Claim(ClaimTypes.Role, u.role.ToString())
                };

                var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(id));

                return LocalRedirect(ReturnUrl!);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return Page();
            }
        }

        // Bot?n ?Entrar como Admin QA (fake)?
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostQuickAdminAsync()
        {
            ReturnUrl ??= Url.Content("~/");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "admin.qa"),
                new Claim(ClaimTypes.Email, "admin.qa@example.com"),
                new Claim(ClaimTypes.Role, "Administrador")
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

            [Required, Display(Name = "Contrase?a")]
            public string Password { get; set; } = "";
        }
    }
}
