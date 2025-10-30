using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProvider.Application.DTOS
{
    public record ProviderCreateDto(
        string FirstName,
        string LastName,
        string? Nit,
        string? Address,
        string? Email,
        string? Phone,
        bool Status = true
    );

    public record ProviderUpdateDto(
        string FirstName,
        string LastName,
        string? Nit,
        string? Address,
        string? Email,
        string? Phone,
        bool Status = true
    );
}
