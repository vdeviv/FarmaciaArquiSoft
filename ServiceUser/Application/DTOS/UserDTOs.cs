using ServiceUser.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUser.Application.DTOS
{
    public record UserCreateDto(
        string FirstName,
        string LastFirstName,
        string LastSecondName,
        string Mail,
        int Phone,
        string Ci,
        UserRole Role
    );

    public record UserUpdateDto(
        string? FirstName,
        string? LastFirstName,
        string? LastSecondName,
        string? Mail,
        int? Phone,
        string? Ci,
        UserRole? Role,
        string? Password
    );

    // Opcional para respuestas limpias
    public record UserViewDto(
        int Id,
        string Username,
        string LastFirstName,
        string? LastSecondName,
        string LastName,
        string? Mail,
        int Phone,
        string Ci,
        UserRole Role
    );
}
