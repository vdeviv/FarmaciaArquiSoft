using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceCommon.Application;
using ServiceCommon.Domain.Ports;
using ServiceLot.Application;
using ServiceLot.Domain;
using System.Security.Cryptography;
using LotEntity = ServiceLot.Domain.Lot;

namespace FarmaView.Pages.Lots
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly LotService _service;
        private readonly IEncryptionService _encryption;

        [BindProperty]
        public LotEntity Input { get; set; } = new LotEntity();

        public EditModel(LotService service, IEncryptionService encryption)
        {
            _service = service;
            _encryption = encryption;
        }

        public async Task<IActionResult> OnGetAsync(string encryptedId)
        {
            if (string.IsNullOrWhiteSpace(encryptedId))
            {
                TempData["ErrorMessage"] = "ID de lote no proporcionado.";
                return RedirectToPage("/Lots/Index");
            }

            int id;
            try { id = _encryption.DecryptId(encryptedId); }
            catch (FormatException)
            {
                TempData["ErrorMessage"] = "ID inválido.";
                return RedirectToPage("/Lots/Index");
            }
            catch (CryptographicException)
            {
                TempData["ErrorMessage"] = "Error de seguridad con el ID.";
                return RedirectToPage("/Lots/Index");
            }

            var found = await _service.GetByIdAsync(id);
            if (found is null)
            {
                TempData["ErrorMessage"] = "Lote no encontrado.";
                return RedirectToPage("/Lots/Index");
            }

            Input = found;
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                var actorId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 1;
                await _service.UpdateAsync(Input, actorId);

                TempData["SuccessMessage"] = "Lote actualizado correctamente.";
                return RedirectToPage("/Lots/Index");
            }
            catch (ServiceLot.Application.ValidationException vex)
            {
                foreach (var kv in vex.Errors)
                    ModelState.AddModelError(kv.Key ?? string.Empty, kv.Value);
                return Page();
            }
            catch (ServiceLot.Application.DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}
