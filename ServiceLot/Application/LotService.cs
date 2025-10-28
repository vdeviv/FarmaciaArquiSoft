using ServiceCommon.Domain.Ports;
using ServiceLot.Domain;
using ServiceLot.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLot.Application
{
    public class LotService
    {
        private readonly IRepository<Lot> _repository;

        public LotService()
        {
            var factory = new LotRepositoryFactory();
            _repository = factory.CreateRepository<Lot>();
        }

        public async Task<IEnumerable<Lot>> GetAllAsync() =>
            await _repository.GetAll();

        public async Task<Lot?> GetByIdAsync(int id) =>
            await _repository.GetById(new Lot { Id = id });

        public async Task<(bool Success, Dictionary<string, string>? Errors)> CreateAsync(Lot lot, int userId = 1)
        {
            // Validación eliminada temporalmente
            lot.CreatedAt = DateTime.Now;
            lot.CreatedBy = userId;

            await _repository.Create(lot);
            return (true, null);
        }

        public async Task<(bool Success, Dictionary<string, string>? Errors)> UpdateAsync(Lot lot, int userId = 1)
        {
            // Validación eliminada temporalmente
            lot.UpdatedAt = DateTime.Now;
            lot.UpdatedBy = userId;

            await _repository.Update(lot);
            return (true, null);
        }

        public async Task<bool> SoftDeleteAsync(int id, int userId = 1)
        {
            var existing = await _repository.GetById(new Lot { Id = id });
            if (existing == null)
                return false;

            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = userId;

            await _repository.Delete(existing);
            return true;
        }
    }
}
