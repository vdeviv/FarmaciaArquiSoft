﻿using ServiceCommon.Domain.Ports;
using ServiceCommon.Domain.Validators;
using ServiceUser.Application.DTOS;
using ServiceUser.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServiceUser.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repo;
        private readonly IValidator<User> _validator;
       private readonly IEmailSender _email;

        public UserService(IRepository<User> repo, IValidator<User> validator, IEmailSender email)
        {
            _repo = repo;
            _validator = validator;
            _email = email;
        }


        public async Task<User> RegisterAsync(UserCreateDto dto, int actorId)
        {
            if (string.IsNullOrWhiteSpace(dto.Mail))
                throw new DomainException("El correo es obligatorio.");

            var user = new User
            {
                first_name = dto.FirstName.Trim(),
                last_first_name = dto.LastFirstName.Trim(),
                last_second_name = dto.LastSecondName.Trim(),
                
                username = "",
                password = "",
                mail = dto.Mail.Trim(),
                phone = dto.Phone,
                ci = dto.Ci.Trim(),
                role = dto.Role,
                created_by = actorId,
                updated_by = actorId,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                is_deleted = false
            };

            var pre = _validator.Validate(user);
            if (!pre.IsSuccess)
                throw new ValidationException(pre.Errors.ToDictionary());


            var existing = await _repo.GetAll();
            var baseUsername = GenerateUsernameFromNames(user.first_name, user.last_first_name, user.last_second_name);
            user.username = EnsureUniqueUsername(baseUsername, existing.Select(x => x.username));

            if (existing.Any(x => x.ci.Equals(user.ci, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("El CI ya existe.");
            if (existing.Any(x => x.mail?.Equals(user.mail, StringComparison.OrdinalIgnoreCase) == true))
                throw new DomainException("El correo ya existe.");

            var plainPassword = GenerateRandomPassword(12);
            user.password = HashPassword(plainPassword);

            var created = await _repo.Create(user);

            var subject = "Tu acceso al sistema de la farmacia";
            var body =
                            $@"Hola {created.first_name},

                        Se creó tu cuenta.
                        Usuario: {created.username}
                        Contraseña temporal: {plainPassword}

                        Por seguridad, cambia la contraseña al ingresar.";

            
            try { await _email.SendAsync(created.mail!, subject, body); }
            catch { /* log y seguir; idealmente usar outbox en el futuro */ }

            return created;
        }

        public async Task<User?> GetByIdAsync(int id)
            => await _repo.GetById(new User { id = id });

        public Task<IEnumerable<User>> ListAsync()
            => _repo.GetAll();

        public async Task UpdateAsync(int id, UserUpdateDto dto, int actorId)
        {
            var current = await GetByIdAsync(id) ?? throw new NotFoundException("Usuario no encontrado.");

            if (dto.FirstName is not null) current.first_name = dto.FirstName.Trim();
            if (dto.LastFirstName is not null) current.last_first_name = dto.LastFirstName.Trim();
            if (dto.LastSecondName is not null) current.last_second_name = dto.LastSecondName.Trim();

            if (dto.Mail is not null) current.mail = dto.Mail.Trim();
            if (dto.Phone is not null) current.phone = dto.Phone.Value;
            if (dto.Ci is not null) current.ci = dto.Ci.Trim();
            if (dto.Role is not null) current.role = dto.Role.Value;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                current.password = HashPassword(dto.Password!);

            current.updated_by = actorId;
            current.updated_at = DateTime.Now;

            var result = _validator.Validate(current);
            if (!result.IsSuccess)
                throw new ValidationException(result.Errors.ToDictionary());


            await _repo.Update(current);
        }

        public async Task SoftDeleteAsync(int id, int actorId)
        {
            var current = await GetByIdAsync(id) ?? throw new NotFoundException("Usuario no encontrado.");
            current.is_deleted = true;
            current.updated_by = actorId;
            current.updated_at = DateTime.Now;
            await _repo.Delete(current);
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            var all = await _repo.GetAll();
            var user = all.FirstOrDefault(u =>
                u.username.Equals(username, StringComparison.OrdinalIgnoreCase) && !u.is_deleted);

            if (user is null || !VerifyPassword(password, user.password))
                throw new DomainException("Credenciales inválidas.");

            return user;
        }

        public bool CanPerformAction(User user, string action)
        {
            if (user.role == UserRole.Administrador) return true;

            var allowed = user.role switch
            {
                UserRole.Cajero => new[] { "Vender", "VerMisDatos" },
                UserRole.Almacenero => new[] { "Almacenar", "VerMisDatos" },
                _ => Array.Empty<string>()
            };
            return allowed.Contains(action, StringComparer.OrdinalIgnoreCase);
        }


        public static string GenerateUsernameFromNames(string first, string firstLast, string secondLast)
        {
            static string Initial(string? s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim()[0].ToString();

            var raw = (Initial(first)  + firstLast+ Initial(secondLast)).ToLowerInvariant();

            var norm = raw.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(norm.Length);
            foreach (var ch in norm)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
            }
            var s = sb.ToString().Normalize(NormalizationForm.FormC);
            // solo [a-z0-9]
            s = Regex.Replace(s, "[^a-z0-9]", "");
            if (s.Length > 20) s = s[..20];
            if (string.IsNullOrEmpty(s)) s = "user";
            return s;
        }

        private static string EnsureUniqueUsername(string baseUsername, IEnumerable<string> existing)
        {
            var set = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
            var candidate = baseUsername;
            var i = 1;
            while (set.Contains(candidate))
            {
                var suffix = i.ToString();
                var head = baseUsername;
                if (head.Length + suffix.Length > 20)
                    head = head[..Math.Max(1, 20 - suffix.Length)];
                candidate = head + suffix;
                i++;
            }
            return candidate;
        }

        private const int Pbkdf2Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
#if NET6_0_OR_GREATER
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, KeySize);
#else
            using var derive = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations);
            var hash = derive.GetBytes(KeySize);
#endif
            return $"PBKDF2|{Pbkdf2Iterations}|{Convert.ToBase64String(salt)}|{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string stored)
        {
            var parts = stored.Split('|');
            if (parts.Length != 4 || parts[0] != "PBKDF2") return false;

            var iterations = int.Parse(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
#if NET6_0_OR_GREATER
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
#else
            using var derive = new Rfc2898DeriveBytes(password, salt, iterations);
            var actual = derive.GetBytes(expected.Length);
#endif

            if (actual.Length != expected.Length) return false;
            var diff = 0;
            for (int i = 0; i < actual.Length; i++) diff |= actual[i] ^ expected[i];
            return diff == 0;
        }
        private static string GenerateRandomPassword(int length)
        {
            if (length < 8) length = 8;

            const string lowers = "abcdefghijklmnopqrstuvwxyz";
            const string uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string symbols = "!@#$%^&*_-";


            var required = new[]
            {
        lowers[RandomNumberGenerator.GetInt32(lowers.Length)],
        uppers[RandomNumberGenerator.GetInt32(uppers.Length)],
        digits[RandomNumberGenerator.GetInt32(digits.Length)],
        symbols[RandomNumberGenerator.GetInt32(symbols.Length)]
    }.ToList();


            string all = lowers + uppers + digits + symbols;
            while (required.Count < length)
                required.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);


            for (int i = required.Count - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (required[i], required[j]) = (required[j], required[i]);
            }

            return new string(required.ToArray());
        }

    }


    public class DomainException : Exception { public DomainException(string m) : base(m) { } }
    public class NotFoundException : Exception { public NotFoundException(string m) : base(m) { } }
    public class ValidationException : Exception
    {
        public Dictionary<string, string> Errors { get; }
        public ValidationException(Dictionary<string, string> errors) : base("Validación de dominio falló.")
            => Errors = errors;
    }
}
