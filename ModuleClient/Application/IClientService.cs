using ServiceClient.Application.DTOS;
using ServiceClient.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceClient.Application
{
    public interface IClientService
    {
        Task<Client> RegisterAsync(ClientCreateDto dto, int actorId);

        Task<Client?> GetByIdAsync(int id);
        Task<IEnumerable<Client>> ListAsync();

        Task UpdateAsync(int id, ClientUpdateDto dto, int actorId);

        Task SoftDeleteAsync(int id, int actorId);
    }
}
