using ServiceClient.Application.DTOS;
using ServiceClient.Domain;
using ServiceCommon.Domain.Ports;
using ServiceCommon.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServiceClient.Application
{
    public class ClientService : IClientService
    {
        private readonly IRepository<Client> _clientRepository;
        private readonly IValidator<Client> _validator;

        public ClientService(IRepository<Client> clientRepository, IValidator<Client> validator)
        {
            _clientRepository = clientRepository;
            _validator = validator;
        }

        private static readonly Regex MultiSpace = new(@"\s+", RegexOptions.Compiled);

        private static string NormalizeName(string? s)
            => string.IsNullOrWhiteSpace(s) ? string.Empty : MultiSpace.Replace(s.Trim(), " ");

        private static string NormalizeEmail(string? s)
            => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim().ToLowerInvariant();

        private static string NormalizeNit(string? s)
            => string.IsNullOrWhiteSpace(s) ? string.Empty : MultiSpace.Replace(s.Trim(), " ");

        public async Task<Client> RegisterAsync(ClientCreateDto dto, int actorId)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var entity = new Client
            {
                first_name = NormalizeName(dto.FirstName),
                last_name = NormalizeName(dto.LastName),
                nit = NormalizeNit(dto.nit),
                email = NormalizeEmail(dto.email),

                is_deleted = false,
                created_by = actorId,
                updated_by = actorId,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            // Validación de dominio (mismo patrón que Users)
            var vr = _validator.Validate(entity);
            if (!vr.IsSuccess)
                throw new ValidationException(vr.Errors.ToDictionary());

            // Reglas de negocio: únicos (ignorando borrados)
            var all = await _clientRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(entity.email) &&
                all.Any(c => !c.is_deleted &&
                             string.Equals(c.email?.Trim(), entity.email, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("El correo ya existe.");

            if (!string.IsNullOrWhiteSpace(entity.nit) &&
                all.Any(c => !c.is_deleted &&
                             string.Equals((c.nit ?? string.Empty).Trim(), entity.nit, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("El NIT ya existe.");

            var created = await _clientRepository.Create(entity);
            return created;
        }

        public async Task<Client?> GetByIdAsync(int id)
            => await _clientRepository.GetById(new Client { id = id });

        public async Task<IEnumerable<Client>> ListAsync()
            => await _clientRepository.GetAll();

        public async Task UpdateAsync(int id, ClientUpdateDto dto, int actorId)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var current = await _clientRepository.GetById(new Client { id = id })
                          ?? throw new NotFoundException($"Cliente con ID {id} no encontrado.");

            if (dto.FirstName is not null) current.first_name = NormalizeName(dto.FirstName);
            if (dto.LastName is not null) current.last_name = NormalizeName(dto.LastName);
            if (dto.nit is not null) current.nit = NormalizeNit(dto.nit);
            if (dto.email is not null) current.email = NormalizeEmail(dto.email);

            current.updated_by = actorId;
            current.updated_at = DateTime.Now;

            var vr = _validator.Validate(current);
            if (!vr.IsSuccess)
                throw new ValidationException(vr.Errors.ToDictionary());

            var all = await _clientRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(current.email) &&
                all.Any(c => c.id != current.id && !c.is_deleted &&
                             string.Equals(c.email?.Trim(), current.email, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("El correo ya existe.");

            if (!string.IsNullOrWhiteSpace(current.nit) &&
                all.Any(c => c.id != current.id && !c.is_deleted &&
                             string.Equals((c.nit ?? string.Empty).Trim(), current.nit, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("El NIT ya existe.");

            await _clientRepository.Update(current);
        }

        public async Task SoftDeleteAsync(int id, int actorId)
        {
            var current = await _clientRepository.GetById(new Client { id = id })
                          ?? throw new NotFoundException($"Cliente con ID {id} no encontrado.");

            current.is_deleted = true;
            current.updated_by = actorId;
            current.updated_at = DateTime.Now;

            await _clientRepository.Delete(current);
        }
    }

    // Excepciones (mismo patrón que Users, pero en el namespace de Clients)
    public class DomainException : Exception { public DomainException(string m) : base(m) { } }
    public class NotFoundException : Exception { public NotFoundException(string m) : base(m) { } }
    public class ValidationException : Exception
    {
        public Dictionary<string, string> Errors { get; }
        public ValidationException(Dictionary<string, string> errors) : base("Validación de dominio falló.")
            => Errors = errors;
    }
}
