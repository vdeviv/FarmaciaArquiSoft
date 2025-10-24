using ServiceClient.Application.DTOS;
using ServiceClient.Domain;
using ServiceCommon.Domain.Ports;
using ServiceUser.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceClient.Application
{
    public class ClientService : IClientService
    {
        private readonly IRepository<Client> _clientRepository; // Usa Client

        public ClientService(IRepository<Client> clientRepository) // Usa Client
        {
            _clientRepository = clientRepository;
        }

        public async Task<Client> RegisterAsync(ClientCreateDto dto, int actorId) // Usa Client
        {
           

            var newClient = new Client // Usa Client
            {
                first_name = dto.FirstName,
                last_name = dto.LastName,
                nit = dto.nit,
                email = dto.email,
                is_deleted = false
            };

            var createdClient = await _clientRepository.Create(newClient);

            return createdClient;
        }

        public async Task<Client?> GetByIdAsync(int id) // Usa Client
        {
            var clientReference = new Client { id = id }; // Usa Client
            return await _clientRepository.GetById(clientReference);
        }

        public async Task<IEnumerable<Client>> ListAsync() // Usa Client
        {
            return await _clientRepository.GetAll();
        }

        public async Task UpdateAsync(int id, ClientUpdateDto dto, int actorId)
        {
            

            var clientReference = new Client { id = id }; // Usa Client
            var existingClient = await _clientRepository.GetById(clientReference);

            if (existingClient == null)
            {
                throw new ArgumentException($"Cliente con ID {id} no encontrado.");
            }

            existingClient.first_name = dto.FirstName;
            existingClient.last_name = dto.LastName;
            existingClient.nit = dto.nit;
            existingClient.email = dto.email;

            await _clientRepository.Update(existingClient);
        }

        public async Task SoftDeleteAsync(int id, int actorId)
        {
           

            var clientReference = new Client { id = id }; // Usa Client
            var existingClient = await _clientRepository.GetById(clientReference);

            if (existingClient == null)
            {
                throw new ArgumentException($"Cliente con ID {id} no encontrado.");
            }

            await _clientRepository.Delete(existingClient);
        }

     

        
    }
}
