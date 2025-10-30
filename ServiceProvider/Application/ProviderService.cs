using ServiceCommon.Domain.Ports;
using ServiceProvider.Application.DTOS;
using ServiceProvider.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProvider.Application
{
    public class ProviderService : IProviderService
    {
        private readonly IRepository<Provider> _providerRepository;

        public ProviderService(IRepository<Provider> providerRepository)
        {
            _providerRepository = providerRepository;
        }

        public async Task<Provider> RegisterAsync(ProviderCreateDto dto, int actorId)
        {
            var entity = new Provider
            {
                first_name = dto.FirstName,
                second_name = dto.SecondName,
                last_first_name = dto.LastFirstName,
                last_second_name = dto.LastSecondName,
                nit = dto.Nit,
                address = dto.Address,
                email = dto.Email,
                phone = dto.Phone,
                status = dto.Status,
                is_deleted = false,
                created_by = actorId
            };

            var created = await _providerRepository.Create(entity);
            return created;
        }

        public Task<Provider?> GetByIdAsync(int id)
            => _providerRepository.GetById(new Provider { id = id });

        public Task<IEnumerable<Provider>> ListAsync()
            => _providerRepository.GetAll();

        public async Task UpdateAsync(int id, ProviderUpdateDto dto, int actorId)
        {
            var existing = await _providerRepository.GetById(new Provider { id = id });
            if (existing is null)
                throw new ArgumentException($"Proveedor con ID {id} no encontrado.");

            existing.first_name = dto.FirstName;
            existing.second_name = dto.SecondName;
            existing.last_first_name = dto.LastFirstName;
            existing.last_second_name = dto.LastSecondName;
            existing.nit = dto.Nit;
            existing.address = dto.Address;
            existing.email = dto.Email;
            existing.phone = dto.Phone;
            existing.status = dto.Status;
            existing.updated_by = actorId;
            existing.updated_at = DateTime.Now;

            await _providerRepository.Update(existing);
        }

        public async Task SoftDeleteAsync(int id, int actorId)
        {
            var existing = await _providerRepository.GetById(new Provider { id = id });
            if (existing is null)
                throw new ArgumentException($"Proveedor con ID {id} no encontrado.");

            existing.updated_by = actorId;
            existing.updated_at = DateTime.Now;

            await _providerRepository.Delete(existing);
        }
    }
}