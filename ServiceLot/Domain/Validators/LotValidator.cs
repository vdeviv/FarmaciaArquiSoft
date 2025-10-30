using FluentResults;
using ServiceCommon.Domain.Ports;
using ServiceCommon.Domain.Validators;
using ServiceLot.Domain;
using System.Text.RegularExpressions;

namespace ServiceLot.Domain.Validators
{
    public class LotValidator : IValidator<Lot>
    {
        private static readonly Regex BatchAllowed =
            new(@"^[A-Za-z0-9\-]+$", RegexOptions.Compiled); // código/lote: letras, números y guion

        public Result Validate(Lot e)
        {
            var r = Result.Ok();


            if (string.IsNullOrWhiteSpace(e.BatchNumber))
                r = r.WithFieldError("BatchNumber", "El número de lote es obligatorio.");
            else
            {
                var b = e.BatchNumber.Trim();
                if (b.Length is < 2 or > 30)
                    r = r.WithFieldError("BatchNumber", "Debe tener entre 2 y 30 caracteres.");
                if (!BatchAllowed.IsMatch(b))
                    r = r.WithFieldError("BatchNumber", "Solo se permiten letras, números y guiones.");
            }

            // Vencimiento: hoy o futuro (ajusta si debe ser estrictamente futuro)
            if (e.ExpirationDate.Date < DateTime.Today)
                r = r.WithFieldError("ExpirationDate", "La fecha de vencimiento no puede estar en el pasado.");

            if (e.Quantity < 0)
                r = r.WithFieldError("Quantity", "La cantidad no puede ser negativa.");

            if (e.UnitCost < 0)
                r = r.WithFieldError("UnitCost", "El costo unitario no puede ser negativo.");

            return r;
        }
    }
}
