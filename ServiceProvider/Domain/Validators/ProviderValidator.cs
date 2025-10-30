using FluentResults;
using ServiceCommon.Domain.Ports;
using ServiceProvider.Domain;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ServiceProvider.Domain.Validators
{
    public class ProviderValidator : IValidator<Provider>
    {
        // Letras + espacios, incluyendo tildes y ñ
        private static readonly Regex LettersAndSpaces =
            new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ ]+$", RegexOptions.Compiled);

        // Email válido básico
        private static readonly Regex Email =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        // NIT: 7–12 dígitos, opcional "-dígito"
        private static readonly Regex Nit =
            new(@"^[0-9]{7,12}(-[0-9]{1})?$", RegexOptions.Compiled);

        // Teléfono: solo dígitos, 6–12 caracteres
        private static readonly Regex Phone =
            new(@"^[0-9]{6,12}$", RegexOptions.Compiled);

        public Result Validate(Provider p)
        {
            var errors = new List<IError>();

            // Primer nombre (requerido)
            if (string.IsNullOrWhiteSpace(p.first_name))
            {
                errors.Add(new Error("El primer nombre es obligatorio.")
                    .WithMetadata("field", "first_name"));
            }
            else
            {
                var v = p.first_name.Trim();
                if (v.Length is < 2 or > 50)
                    errors.Add(new Error("Debe tener entre 2 y 50 caracteres.")
                        .WithMetadata("field", "first_name"));
                if (!LettersAndSpaces.IsMatch(v))
                    errors.Add(new Error("Solo debe contener letras y espacios.")
                        .WithMetadata("field", "first_name"));
            }

            // Segundo nombre (opcional)
            if (!string.IsNullOrWhiteSpace(p.second_name))
            {
                var v = p.second_name.Trim();
                if (v.Length > 50)
                    errors.Add(new Error("No debe exceder 50 caracteres.")
                        .WithMetadata("field", "second_name"));
                if (!LettersAndSpaces.IsMatch(v))
                    errors.Add(new Error("Solo debe contener letras y espacios.")
                        .WithMetadata("field", "second_name"));
            }

            // Apellido paterno (requerido)
            if (string.IsNullOrWhiteSpace(p.last_first_name))
            {
                errors.Add(new Error("El apellido paterno es obligatorio.")
                    .WithMetadata("field", "last_first_name"));
            }
            else
            {
                var v = p.last_first_name.Trim();
                if (v.Length is < 2 or > 50)
                    errors.Add(new Error("Debe tener entre 2 y 50 caracteres.")
                        .WithMetadata("field", "last_first_name"));
                if (!LettersAndSpaces.IsMatch(v))
                    errors.Add(new Error("Solo debe contener letras y espacios.")
                        .WithMetadata("field", "last_first_name"));
            }

            // Apellido materno (opcional)
            if (!string.IsNullOrWhiteSpace(p.last_second_name))
            {
                var v = p.last_second_name.Trim();
                if (v.Length > 50)
                    errors.Add(new Error("No debe exceder 50 caracteres.")
                        .WithMetadata("field", "last_second_name"));
                if (!LettersAndSpaces.IsMatch(v))
                    errors.Add(new Error("Solo debe contener letras y espacios.")
                        .WithMetadata("field", "last_second_name"));
            }

            // Email (opcional)
            if (!string.IsNullOrWhiteSpace(p.email))
            {
                var mail = p.email.Trim();
                if (mail.Length > 150)
                    errors.Add(new Error("No debe exceder 150 caracteres.")
                        .WithMetadata("field", "email"));
                if (!Email.IsMatch(mail))
                    errors.Add(new Error("Formato de correo inválido.")
                        .WithMetadata("field", "email"));
            }

            // NIT (opcional)
            if (!string.IsNullOrWhiteSpace(p.nit))
            {
                var v = p.nit.Trim();
                if (!Nit.IsMatch(v))
                    errors.Add(new Error("El NIT debe tener 7–12 dígitos (opcional -dígito).")
                        .WithMetadata("field", "nit"));
            }

            // Teléfono (opcional)
            if (!string.IsNullOrWhiteSpace(p.phone))
            {
                var v = p.phone.Trim();
                if (!Phone.IsMatch(v))
                    errors.Add(new Error("El teléfono debe tener entre 6 y 12 dígitos numéricos.")
                        .WithMetadata("field", "phone"));
            }

            return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
        }
    }
}
