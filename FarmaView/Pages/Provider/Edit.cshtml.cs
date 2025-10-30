using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ServiceCommon.Domain.Ports;
using ServiceProvider.Application;
using ServiceProvider.Application.DTOS;
using System.Security.Cryptography;
using ProviderEntity = ServiceProvider.Domain.Provider;

namespace FarmaView.Pages.Provider
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly IProviderService _providerService;
        private readonly IEncryptionService _encryptionService;

        public EditModel(IProviderService providerService, IEncryptionService encryptionService)
        {
            _providerService = providerService;
            _encryptionService = encryptionService;
        }

        [BindProperty]
        public ProviderEntity Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] = "ID de proveedor no proporcionado.";
                return RedirectToPage("/Provider/IndexProvider");
            }

            int providerId;
            try
            {
                providerId = _encryptionService.DecryptId(id);
            }
            catch (FormatException)
            {
                TempData["ErrorMessage"] = "Formato de ID inválido.";
                return RedirectToPage("/Provider/IndexProvider");
            }
            catch (CryptographicException)
            {
                TempData["ErrorMessage"] = "Error de seguridad con el ID.";
                return RedirectToPage("/Provider/IndexProvider");
            }

            var found = await _providerService.GetByIdAsync(providerId);
            if (found is null)
            {
                TempData["ErrorMessage"] = "Proveedor no encontrado o eliminado.";
                return RedirectToPage("/Provider/IndexProvider");
            }

            Input = found;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var dto = new ProviderUpdateDto(
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

                await _providerService.UpdateAsync(Input.id, dto, 1); // actorId temporal
                TempData["SuccessMessage"] = "Proveedor actualizado correctamente.";
                return RedirectToPage("/Provider/IndexProvider");
            }
            catch (ValidationException vex)
            {
                // Mostrar solo debajo de cada campo
                foreach (var error in vex.Errors)
                {
                    var hasField = error.Metadata != null &&
                                   error.Metadata.TryGetValue("field", out var f) &&
                                   f is string s && !string.IsNullOrWhiteSpace(s);

                    if (hasField)
                        ModelState.AddModelError($"Input.{(string)error.Metadata["field"]}", error.Message);
                    else
                        ModelState.AddModelError(string.Empty, error.Message);
                }
                return Page();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                // Duplicado: NIT o Email
                // Si quieres, puedes mapearlo a un campo específico, por simplicidad lo dejamos general
                ModelState.AddModelError("Input.nit", "NIT o Email ya están registrados para otro proveedor.");
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
