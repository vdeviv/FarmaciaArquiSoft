using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceCommon.Application;           // IEncryptionService
using ServiceCommon.Domain.Ports;
using ServiceLot.Application;
using ServiceLot.Domain;
using System.Security.Cryptography;

namespace FarmaView.Pages.Lots
{
    public class IndexModel : PageModel
    {
        private readonly LotService _service;
        private readonly IEncryptionService _encryption;

        public IList<Lot> Lots { get; private set; } = new List<Lot>();

        public IndexModel(LotService service, IEncryptionService encryption)
        {
            _service = service;
            _encryption = encryption;
        }

        public async Task OnGetAsync()
        {
            Lots = (await _service.GetAllAsync()).ToList();
        }

        // El formulario del modal debe enviar un campo hidden con name="encryptedId"
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(string encryptedId)
        {
            if (string.IsNullOrWhiteSpace(encryptedId))
            {
                TempData["ErrorMessage"] = "ID de lote no proporcionado.";
                return RedirectToPage();
            }

            int id;
            try
            {
                id = _encryption.DecryptId(encryptedId);
            }
            catch (FormatException)
            {
                TempData["ErrorMessage"] = "ID inválido.";
                return RedirectToPage();
            }
            catch (CryptographicException)
            {
                TempData["ErrorMessage"] = "Error de seguridad con el ID.";
                return RedirectToPage();
            }

            try
            {
                await _service.SoftDeleteAsync(id);
                TempData["SuccessMessage"] = "Lote eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"No se pudo eliminar el lote: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
