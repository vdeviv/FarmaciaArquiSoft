using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceLot.Infrastructure;
using ServiceCommon.Domain.Ports;
using ServiceLot.Application;
using ServiceLot.Domain;
using LotEntity = ServiceLot.Domain.Lot;

namespace FarmaView.Pages.Lots
{
    public class CreateModel : PageModel
    {
        private readonly LotService _service;

        [BindProperty]
        public LotEntity Lot { get; set; } = new LotEntity();

        public CreateModel()
        {
            _service = new LotService();
        }

        public void OnGet()
        {
            // Página de creación sin lógica adicional
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var (success, _) = await _service.CreateAsync(Lot);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Error al crear el lote.");
                return Page();
            }

            TempData["SuccessMessage"] = "Lote creado exitosamente.";
            return RedirectToPage("Index");
        }
    }
}
