using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ServiceCommon.Domain.Ports;
using ServiceProvider.Infraestructure;
using System.Security.Cryptography;

using ProviderEntity = ServiceProvider.Domain.Provider;

namespace FarmaView.Pages.Provider
{ 
        [BindProperties]
    public class EditModel : PageModel
    {
        private readonly IRepository<ProviderEntity> _providerRepository;
        private readonly IEncryptionService _encryptionService;

        [BindProperty]
        public ProviderEntity Input { get; set; } = new();

        public EditModel(IEncryptionService encryptionService)
        {
            var factory = new ProviderRepositoryFactory();
            _providerRepository = factory.CreateRepository<ProviderEntity>();
            _encryptionService = encryptionService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Error"] = "ID de proveedor no proporcionado.";
                return RedirectToPage("/Provider/IndexProvider");
            }

            int decryptedId;
            try
            {
                decryptedId = _encryptionService.DecryptId(id);
            }
            catch (FormatException)
            {
                TempData["Error"] = "Formato de ID inválido.";
                return RedirectToPage("/Provider/IndexProvider");
            }
            catch (CryptographicException)
            {
                TempData["Error"] = "Error de seguridad con el ID.";
                return RedirectToPage("/Provider/IndexProvider");
            }

            var temp = new ProviderEntity { id = decryptedId };
            var found = await _providerRepository.GetById(temp);

            if (found is null)
            {
                TempData["Error"] = "Proveedor no encontrado o eliminado.";
                return RedirectToPage("/Provider/IndexProvider");
            }

            Input = found;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                await _providerRepository.Update(Input);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ModelState.AddModelError(string.Empty, "NIT o Email ya están registrados para otro proveedor.");
                return Page();
            }

            TempData["Success"] = "Proveedor actualizado correctamente.";
            return RedirectToPage("/Provider/IndexProvider");
        }
    }
}
