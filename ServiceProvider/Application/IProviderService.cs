using ServiceProvider.Application.DTOS;
using ServiceProvider.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProvider.Application
{
    public interface IProviderService
    {
        Task<Provider> RegisterAsync(ProviderCreateDto dto, int actorId);
        Task<Provider?> GetByIdAsync(int id);
        Task<IEnumerable<Provider>> ListAsync();
        Task UpdateAsync(int id, ProviderUpdateDto dto, int actorId);
        Task SoftDeleteAsync(int id, int actorId);
    }
}
