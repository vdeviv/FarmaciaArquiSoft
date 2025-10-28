using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ServiceCommon.Domain.Ports;
using System.Security.Cryptography;
using ServiceLot.Infrastructure; 
using ServiceLot.Application;
using LotEntity = ServiceLot.Domain.Lot;

namespace FarmaView.Pages.Lots
{
    [BindProperties]
    [Route("Lots/Edit/{encryptedId?}")]
    public class EditModel : PageModel
    {
        private readonly IRepository<LotEntity> _lotRepository;
        private readonly IEncryptionService _encryptionService;

        [BindProperty]
        public LotEntity Input { get; set; } = new LotEntity();

        public EditModel(IEncryptionService encryptionService)
        {
            var factory = new LotRepositoryFactory();
            _lotRepository = factory.CreateRepository<LotEntity>();
            _encryptionService = encryptionService;
        }

        public async Task<IActionResult> OnGetAsync(string encryptedId)
        {
            if (string.IsNullOrEmpty(encryptedId))
            {
                TempData["ErrorMessage"] = "ID de lote no proporcionado.";
                return RedirectToPage("/Lots/Index");
            }

            int id;
            try
            {
                id = _encryptionService.DecryptId(encryptedId);
            }
            catch (FormatException)
            {
                TempData["ErrorMessage"] = "ID de lote inválido o corrupto.";
                return RedirectToPage("/Lots/Index");
            }
            catch (CryptographicException)
            {
                TempData["ErrorMessage"] = "Error de seguridad al desencriptar el ID.";
                return RedirectToPage("/Lots/Index");
            }

            var tempLot = new LotEntity { Id = id };
            var found = await _lotRepository.GetById(tempLot);

            if (found is null)
            {
                TempData["ErrorMessage"] = "Lote no encontrado o eliminado.";
                return RedirectToPage("/Lots/Index");
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
                await _lotRepository.Update(Input);
            }
            catch (MySqlException ex) when (ex.Number == 1062) 
            {
                ModelState.AddModelError("Input.Name", "Ya existe un lote con este nombre o código.");
                return Page();
            }

            TempData["SuccessMessage"] = "Lote actualizado correctamente.";
            return RedirectToPage("/Lots/Index");
        }
    }
}
