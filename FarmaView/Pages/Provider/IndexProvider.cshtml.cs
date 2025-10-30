using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceCommon.Domain.Ports;
using ServiceProvider.Application;

using ProviderEntity = ServiceProvider.Domain.Provider;

namespace FarmaView.Pages.Provider
{
    public class IndexProviderModel : PageModel
    {
        private readonly IProviderService _providerService;
        private readonly IEncryptionService _encryptionService;

        public IEnumerable<ProviderEntity> Providers { get; set; } = new List<ProviderEntity>();
        public Dictionary<int, string> EncryptedIds { get; set; } = new();

        public IndexProviderModel(IProviderService providerService, IEncryptionService encryptionService)
        {
            _providerService = providerService;
            _encryptionService = encryptionService;
        }

        public async Task OnGetAsync()
        {
            Providers = await _providerService.ListAsync();
            EncryptedIds = Providers.ToDictionary(p => p.id, p => _encryptionService.EncryptId(p.id));
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                
                const int ACTOR_ID = 1;
                await _providerService.SoftDeleteAsync(id, ACTOR_ID);
                TempData["SuccessMessage"] = "Proveedor eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"No se pudo eliminar el proveedor: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}
