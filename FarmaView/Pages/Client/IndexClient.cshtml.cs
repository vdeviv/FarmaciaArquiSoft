using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceClient.Application;
using ServiceCommon.Domain.Ports;
using ServiceClient.Domain;

using ClientEntity = ServiceClient.Domain.Client;

namespace FarmaView.Pages.Client
{
    public class IndexClientModel : PageModel
    {
        private readonly IClientService _clientService;

        private readonly IEncryptionService _encryptionService;


        public IEnumerable<ClientEntity> Clients { get; set; } = new List<ClientEntity>();


        public Dictionary<int, string> EncryptedIds { get; set; } = new Dictionary<int, string>();


        public IndexClientModel(IClientService clientService, IEncryptionService encryptionService)
        {
            _clientService = clientService;
            _encryptionService = encryptionService;
        }

        public async Task OnGetAsync()
        {
            Clients = await _clientService.ListAsync();


            foreach (var client in Clients)
            {
                var encryptedId = _encryptionService.EncryptId(client.id);

                EncryptedIds.Add(client.id, encryptedId);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            const int ACTOR_ID = 1;

            try
            {
                await _clientService.SoftDeleteAsync(id, ACTOR_ID);
                TempData["SuccessMessage"] = $"Cliente eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió un error al eliminar: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
