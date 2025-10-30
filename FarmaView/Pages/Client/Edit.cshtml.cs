
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceClient.Application;             // IClientService + excepciones del servicio
using ServiceClient.Application.DTOS;        // ClientUpdateDto
using ServiceClient.Domain;                  // ClientEntity
using ServiceCommon.Application;             // IEncryptionService
using ServiceCommon.Domain.Ports;
using System.Security.Cryptography;

using ClientEntity = ServiceClient.Domain.Client;

namespace FarmaView.Pages.Client
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly IClientService _clients;
        private readonly IEncryptionService _encryptionService;

        [BindProperty]
        public ClientEntity Input { get; set; } = new ClientEntity();

        public EditModel(IClientService clients, IEncryptionService encryptionService)
        {
            _clients = clients;
            _encryptionService = encryptionService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Error"] = "ID de cliente no proporcionado.";
                return RedirectToPage("/Client/IndexClient");
            }

            int decryptedId;
            try
            {
                decryptedId = _encryptionService.DecryptId(id);
            }
            catch (FormatException)
            {
                TempData["Error"] = "Formato de ID inválido. Posible manipulación de URL.";
                return RedirectToPage("/Client/IndexClient");
            }
            catch (CryptographicException)
            {
                TempData["Error"] = "Error de seguridad. Posible manipulación de URL.";
                return RedirectToPage("/Client/IndexClient");
            }

            var found = await _clients.GetByIdAsync(decryptedId);
            if (found is null)
            {
                TempData["Error"] = "Cliente no encontrado o eliminado.";
                return RedirectToPage("/Client/IndexClient");
            }

            Input = found;
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Construimos el DTO igual que en Users (Create/Update)
            var dto = new ClientUpdateDto(
                FirstName: Input.first_name,
                LastName: Input.last_name,
                email: Input.email,
                nit: Input.nit
            );

            var actorId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 1;

            try
            {
                await _clients.UpdateAsync(Input.id, dto, actorId);
                TempData["Success"] = "Cliente actualizado correctamente.";
                return RedirectToPage("/Client/IndexClient");
            }
            catch (ServiceClient.Application.ValidationException vex)
            {
                // Mismo patrón que Users: errores al resumen de arriba
                foreach (var kv in vex.Errors)
                    ModelState.AddModelError(kv.Key ?? string.Empty, kv.Value);
                return Page();
            }
            catch (ServiceClient.Application.DomainException ex)
            {
                // Reglas de negocio (únicos: email/nit) → también arriba
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error al actualizar: {ex.Message}");
                return Page();
            }
        }
    }
}
