using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ServiceProvider.Application;
using ServiceProvider.Application.DTOS;
using ProviderEntity = ServiceProvider.Domain.Provider;

namespace FarmaView.Pages.Provider
{
    [BindProperties]
    public class CreateModel : PageModel    
    {
        private readonly IProviderService _providerService;

        public CreateModel(IProviderService providerService)
        {
            _providerService = providerService;
        }

        [BindProperty]
        public ProviderEntity Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var dto = new ProviderCreateDto(
                    Input.first_name,
                    Input.second_name,
                    Input.last_first_name,
                    Input.last_second_name,
                    Input.nit,
                    Input.address,
                    Input.email,
                    Input.phone,
                    Input.status
                );

                await _providerService.RegisterAsync(dto, 1); // actorId temporal
                TempData["SuccessMessage"] = "Proveedor creado correctamente.";
                return RedirectToPage("/Provider/IndexProvider");
            }
            catch (ValidationException vex)
            {
                // Mapear errores de validación al ModelState
                foreach (var error in vex.Errors)
                {
                    var field = error.Metadata != null && error.Metadata.TryGetValue("field", out var f)
                        ? f as string
                        : null;

                    if (!string.IsNullOrWhiteSpace(field))
                        ModelState.AddModelError($"Input.{field}", error.Message);
                    else
                        ModelState.AddModelError(string.Empty, error.Message);
                }

                return Page();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ModelState.AddModelError(string.Empty, "NIT o Email ya están registrados para otro proveedor.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error inesperado: {ex.Message}");
                return Page();
            }
        }
    }
}
