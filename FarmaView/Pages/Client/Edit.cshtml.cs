using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ServiceCommon.Domain.Ports;
using System.Security.Cryptography;
using ServiceClient.Infrastructure; // Necesario si ClientRepositoryFactory está aquí (revisar)

using ClientEntity = ServiceClient.Domain.Client;

namespace FarmaView.Pages.Client
{
    [BindProperties]
    public class EditModel : PageModel
    {
        // El repositorio ahora se inyecta por el constructor (mejor práctica)
        // PERO para mantener la lógica original de usar la Factory:
        private readonly IRepository<ClientEntity> _ClientRepository;
        private readonly IEncryptionService _encryptionService;

        // Se elimina la dependencia del IValidator<ClientEntity>
        // private readonly IValidator<ClientEntity> _validator; 

        [BindProperty]
        public ClientEntity Input { get; set; } = new ClientEntity();

        // Se inyecta IEncryptionService, pero se mantiene la Factory en el constructor 
        // para el repositorio, siguiendo tu lógica anterior.
        public EditModel(IEncryptionService encryptionService)
        {
            // Se elimina la asignación de _validator

            var factory = new ClientRepositoryFactory();
            _ClientRepository = factory.CreateRepository<ClientEntity>();

            _encryptionService = encryptionService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID de cliente no proporcionado.";
                return RedirectToPage("/Client/IndexClient");
            }

            int decryptedId;
            try
            {
                decryptedId = _encryptionService.DecryptId(id);
            }
            catch (FormatException)
            {
                TempData["Error"] = "Formato de ID inválido. Posible manipulación de URL.";
                return RedirectToPage("/Client/IndexClient");
            }
            catch (CryptographicException)
            {
                TempData["Error"] = "Error de seguridad. Posible manipulación de URL.";
                return RedirectToPage("/Client/IndexClient");
            }

            var tempClient = new ClientEntity { id = decryptedId };
            var found = await _ClientRepository.GetById(tempClient);

            if (found is null)
            {
                TempData["Error"] = $"Cliente no encontrado o eliminado.";
                return RedirectToPage("/Client/IndexClient");
            }

            Input = found;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Únicamente validamos el modelo usando Data Annotations (ModelState.IsValid)
            if (!ModelState.IsValid) return Page();

            // ❌ Eliminado: Se quitó toda la lógica de validación con _validator.Validate(Input)

            try
            {
                // El campo Input.id (oculto en la vista Edit.cshtml) ya contiene el ID desencriptado.
                await _ClientRepository.Update(Input);
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
            {
                ModelState.AddModelError("Input.email", "Ese email ya está vinculado a otro cliente. Por favor, usa uno distinto.");
                return Page();
            }

            TempData["Success"] = "Cliente actualizado correctamente.";
            return RedirectToPage("/Client/IndexClient");
        }
    }
}