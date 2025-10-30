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
            
            var factory = new ProviderRepositoryFactory();
            _providerRepository = factory.CreateRepository<ProviderEntity>();
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                
                Input.is_deleted = false;
                if (Request.Form["Input.status"].Count == 0)
                    Input.status = true;

                
                Input.created_by = 1;

                await _providerRepository.Create(Input);
                TempData["SuccessMessage"] = "Proveedor creado correctamente.";
                return RedirectToPage("/Provider/IndexProvider");
            }
            catch (MySqlException ex) when (ex.Number == 1062) 
            {
                
                ModelState.AddModelError(string.Empty, "NIT o Email ya están registrados para otro proveedor.");
                return Page();
            }
        }
    }
}