using ServiceCommon.Domain.Ports;
using ServiceCommon.Domain.Validators;
using ServiceLot.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLot.Application
{
    public class LotService
    {
        private readonly IRepository<Lot> _repo;
        private readonly IValidator<Lot> _validator;

        public LotService(IRepository<Lot> repo, IValidator<Lot> validator)
        {
            _repo = repo;
            _validator = validator;
        }

        public async Task<IEnumerable<Lot>> GetAllAsync() => await _repo.GetAll();

        public async Task<Lot?> GetByIdAsync(int id) => await _repo.GetById(new Lot { Id = id });

        public async Task<Lot> CreateAsync(Lot lot, int actorId = 1)
        {
            // Normalizaci�n
            lot.BatchNumber = lot.BatchNumber?.Trim() ?? string.Empty;

            lot.IsDeleted = false;
            lot.CreatedBy = actorId;
            lot.UpdatedBy = actorId;
            lot.CreatedAt = DateTime.Now;
            lot.UpdatedAt = DateTime.Now;

            // Validaci�n de dominio
            var vr = _validator.Validate(lot);
            if (!vr.IsSuccess)
                throw new ValidationException(vr.Errors.ToDictionary());

            // �nico por medicina + n�mero de lote (ignora borrados)
            var all = await _repo.GetAll();
            if (all.Any(x => !x.IsDeleted
                             && x.MedicineId == lot.MedicineId
                             && x.BatchNumber.Equals(lot.BatchNumber, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("Ya existe un lote con ese n�mero para la misma medicina.");

            return await _repo.Create(lot);
        }

        public async Task UpdateAsync(Lot lot, int actorId = 1)
        {
            var current = await _repo.GetById(new Lot { Id = lot.Id })
                          ?? throw new NotFoundException("Lote no encontrado.");

            // Map + normalizaci�n
            current.MedicineId = lot.MedicineId;
            current.BatchNumber = (lot.BatchNumber ?? "").Trim();
            current.ExpirationDate = lot.ExpirationDate;
            current.Quantity = lot.Quantity;
            current.UnitCost = lot.UnitCost;

            current.UpdatedAt = DateTime.Now;
            current.UpdatedBy = actorId;

            // Validaci�n
            var vr = _validator.Validate(current);
            if (!vr.IsSuccess)
                throw new ValidationException(vr.Errors.ToDictionary());

            // �nico por medicina + n�mero de lote (excluye el propio)
            var all = await _repo.GetAll();
            if (all.Any(x => x.Id != current.Id && !x.IsDeleted
                             && x.MedicineId == current.MedicineId
                             && x.BatchNumber.Equals(current.BatchNumber, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("Ya existe otro lote con ese n�mero para la misma medicina.");

            await _repo.Update(current);
        }

        public async Task SoftDeleteAsync(int id, int actorId = 1)
        {
            var existing = await _repo.GetById(new Lot { Id = id })
                           ?? throw new NotFoundException("Lote no encontrado.");

            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = actorId;

            await _repo.Delete(existing);
        }
    }

    // Excepciones iguales al patr�n que ya usas
    public class DomainException : Exception { public DomainException(string m) : base(m) { } }
    public class NotFoundException : Exception { public NotFoundException(string m) : base(m) { } }
    public class ValidationException : Exception
    {
        public Dictionary<string, string> Errors { get; }
        public ValidationException(Dictionary<string, string> errors) : base("Validaci�n de dominio fall�.")
            => Errors = errors;
    }
}
