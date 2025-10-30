using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ServiceCommon.Domain.Ports;
using ServiceProvider.Infraestructure;

using ProviderEntity = ServiceProvider.Domain.Provider;

namespace FarmaView.Pages.Provider
{
    [BindProperties]
    public class CreateModel : PageModel
    {
        private readonly IRepository<ProviderEntity> _providerRepository;

        [BindProperty]
        public ProviderEntity Input { get; set; } = new();

        public CreateModel()
        {
            // Igual que Client: usar Factory directa
            var factory = new ProviderRepositoryFactory();
            _providerRepository = factory.CreateRepository<ProviderEntity>();
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                // status por defecto true (activo) si no vino del form
                if (Input.status == false && Request.Form["Input.status"].Count == 0)
                    Input.status = true;

                await _providerRepository.Create(Input);
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
            {
                // Duplicados posibles: nit, email
                ModelState.AddModelError(string.Empty, "NIT o Email ya están registrados para otro proveedor.");
                return Page();
            }

            TempData["SuccessMessage"] = "Proveedor creado correctamente.";
            return RedirectToPage("/Provider/IndexProvider");
        }
    }
}
