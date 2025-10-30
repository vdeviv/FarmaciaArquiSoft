namespace ServiceProvider.Application.DTOS
{
    public record ProviderCreateDto(
        string FirstName,
        string? SecondName,
        string LastFirstName,
        string? LastSecondName,
        string? Nit,
        string? Address,
        string? Email,
        string? Phone,
        bool Status = true
    );

    public record ProviderUpdateDto(
        string FirstName,
        string? SecondName,
        string LastFirstName,
        string? LastSecondName,
        string? Nit,
        string? Address,
        string? Email,
        string? Phone,
        bool Status = true
    );
}
