using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLot.Application;
using ServiceCommon.Domain.Ports;
using LotEntity = ServiceLot.Domain.Lot;

namespace FarmaView.Pages.Lots
{
    public class IndexModel : PageModel
    {
        private readonly LotService _service;
        private readonly IEncryptionService _encryptionService;

        public IEnumerable<LotEntity> Lots { get; set; } = new List<LotEntity>();

        public IndexModel(IEncryptionService encryptionService)
        {
            _service = new LotService(); // sin validator
            _encryptionService = encryptionService;
        }

        public async Task OnGetAsync()
        {
            Lots = await _service.GetAllAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string encryptedId)
        {
            int id;
            try
            {
                id = _encryptionService.DecryptId(encryptedId);
            }
            catch (FormatException)
            {
                TempData["ErrorMessage"] = "ID de lote inválido o corrupto.";
                return RedirectToPage();
            }

            var success = await _service.SoftDeleteAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = "Error al eliminar el lote. El lote no fue encontrado.";
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Lote eliminado correctamente.";
            return RedirectToPage();
        }
    }
}
