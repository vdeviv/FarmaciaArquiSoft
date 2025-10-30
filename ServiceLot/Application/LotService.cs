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
            // Normalización
            lot.BatchNumber = lot.BatchNumber?.Trim() ?? string.Empty;

            lot.IsDeleted = false;
            lot.CreatedBy = actorId;
            lot.UpdatedBy = actorId;
            lot.CreatedAt = DateTime.Now;
            lot.UpdatedAt = DateTime.Now;

            // Validación de dominio
            var vr = _validator.Validate(lot);
            if (!vr.IsSuccess)
                throw new ValidationException(vr.Errors.ToDictionary());

            // ⚠️ Corregido: El script SQL tiene UNIQUE(batch_number) global, sin importar medicine_id o is_deleted.
            var all = await _repo.GetAll();
            if (all.Any(x => x.BatchNumber.Equals(lot.BatchNumber, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("Ya existe un lote con ese número (el número de lote debe ser único globalmente según la base de datos).");

            return await _repo.Create(lot);
        }

        public async Task UpdateAsync(Lot lot, int actorId = 1)
        {
            var current = await _repo.GetById(new Lot { Id = lot.Id })
                          ?? throw new NotFoundException("Lote no encontrado.");

            // Map + normalización
            // MedicineId ya es 'int' non-nullable
            current.MedicineId = lot.MedicineId;
            current.BatchNumber = (lot.BatchNumber ?? "").Trim();
            current.ExpirationDate = lot.ExpirationDate;
            current.Quantity = lot.Quantity;
            current.UnitCost = lot.UnitCost;

            current.UpdatedAt = DateTime.Now;
            current.UpdatedBy = actorId;

            // Validación
            var vr = _validator.Validate(current);
            if (!vr.IsSuccess)
                throw new ValidationException(vr.Errors.ToDictionary());

            // ⚠️ Corregido: El script SQL tiene UNIQUE(batch_number) global.
            // Se verifica la unicidad global excluyendo solo el propio lote (current.Id).
            var all = await _repo.GetAll();
            if (all.Any(x => x.Id != current.Id
                             && x.BatchNumber.Equals(current.BatchNumber, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("Ya existe otro lote con ese número (el número de lote debe ser único globalmente según la base de datos).");

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

    // Excepciones iguales al patrón que ya usas
    public class DomainException : Exception { public DomainException(string m) : base(m) { } }
    public class NotFoundException : Exception { public NotFoundException(string m) : base(m) { } }
    public class ValidationException : Exception
    {
        public Dictionary<string, string> Errors { get; }
        public ValidationException(Dictionary<string, string> errors) : base("Validación de dominio falló.")
            => Errors = errors;
    }
}