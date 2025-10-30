using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceClient.Application;            // IClientService + excepciones (Domain/Validation)
using ServiceClient.Application.DTOS;
using ClientEntity = ServiceClient.Domain.Client;

namespace FarmaView.Pages.Client
{
    public class CreateModel : PageModel
    {
        private readonly IClientService _clients;

        public CreateModel(IClientService clients)
        {
            _clients = clients;
        }

        [BindProperty]
        public ClientEntity Input { get; set; } = new ClientEntity();

        public void OnGet() { }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var dto = new ClientCreateDto(
                FirstName: Input.first_name,
                LastName: Input.last_name,
                email: Input.email,
                nit: Input.nit
            );

            // Igual que en Users: usa un actor fijo o del claim si ya lo tienes
            var actorId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 1;

            try
            {
                await _clients.RegisterAsync(dto, actorId);
                TempData["SuccessMessage"] = "Cliente creado correctamente.";
                return RedirectToPage("/Client/IndexClient");
            }
            catch (ServiceClient.Application.ValidationException vex)
            {
                // Igual que Users: mostrar arriba (summary). No prefijamos "Input."
                foreach (var kv in vex.Errors)
                    ModelState.AddModelError(kv.Key ?? string.Empty, kv.Value);

                return Page();
            }
            catch (ServiceClient.Application.DomainException ex)
            {
                // Reglas de negocio (únicos, etc.) → también arriba
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ocurrió un error al crear el cliente: {ex.Message}");
                return Page();
            }
        }
    }
}
