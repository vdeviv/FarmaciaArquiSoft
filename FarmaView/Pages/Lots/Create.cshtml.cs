using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public CreateModel(LotService service)
        {
            _service = service;
        }

        public void OnGet(int? medicineId)
        {
            if (medicineId.HasValue && medicineId.Value > 0)
                Lot.MedicineId = medicineId.Value;
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {

            if (!ModelState.IsValid) return Page();

            try
            {
                var actorId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 1;
                await _service.CreateAsync(Lot, actorId);

                TempData["SuccessMessage"] = "Lote creado exitosamente.";
                return RedirectToPage("Index");
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
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error al crear el lote: {ex.Message}");
                return Page();
            }
        }
    }
}
