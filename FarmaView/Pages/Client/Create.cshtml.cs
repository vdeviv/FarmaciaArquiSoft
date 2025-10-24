using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ServiceClient.Infrastructure;
using ServiceCommon.Domain.Ports;

using ClientEntity = ServiceClient.Domain.Client;

namespace FarmaView.Pages.Client
{
    // Se añade [BindProperties] para que Input se cargue automáticamente
    [BindProperties]
    public class CreateModel : PageModel
    {
        private readonly IRepository<ClientEntity> _ClientRepository; // Usa ClientEntity

        // ❌ Eliminado: private readonly IValidator<ClientEntity> _validator; 

        [BindProperty]
        public ClientEntity Input { get; set; } = new ClientEntity { }; // Usa ClientEntity

        // Se elimina la inyección de IValidator y se inicializa el repositorio
        public CreateModel()
        {
            // Se elimina la asignación de _validator

            var factory = new ClientRepositoryFactory();
            _ClientRepository = factory.CreateRepository<ClientEntity>();
        }

        public void OnGet()
        {
            // Este método se deja vacío intencionalmente.
        }


        // ✅ Se deja [ValidateAntiForgeryToken] ya que esta es la acción de POST
        public async Task<IActionResult> OnPostAsync()
        {
            // Paso 1: Únicamente validamos el modelo usando Data Annotations (ModelState.IsValid)
            if (!ModelState.IsValid) return Page();

            // ❌ Eliminado: Se quitó toda la lógica de validación con _validator.Validate(Input)

            try
            {
                // Paso 2: Crear el registro en la base de datos
                await _ClientRepository.Create(Input);
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Error 1062: Entrada duplicada (ej. email)
            {
                // Paso 3: Manejar la excepción de duplicidad específica de MySQL
                ModelState.AddModelError("Input.email", "Ese email ya se encuentra vinculado a otro cliente. Por favor, usa uno distinto.");
                return Page();
            }

            TempData["SuccessMessage"] = "Cliente creado correctamente.";
            return RedirectToPage("/Client/IndexClient");
        }
    }
}