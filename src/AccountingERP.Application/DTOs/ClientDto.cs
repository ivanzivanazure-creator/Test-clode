namespace AccountingERP.Application.DTOs;

using AccountingERP.Domain.Aggregates.Client;

public sealed record ClientDto(
    int        Id,
    int        TenantId,
    string     Name,
    string?    PIB,
    string?    MaticniBroj,
    string?    Address,
    string?    City,
    string     Country,
    string?    IBAN,
    string?    Email,
    string?    Phone,
    bool       IsActive,
    ClientType ClientType)
{
    public static ClientDto FromDomain(Client c) => new(
        c.Id, c.TenantId, c.Name, c.PIB, c.MaticniBroj,
        c.Address, c.City, c.Country, c.IBAN, c.Email, c.Phone,
        c.IsActive, c.ClientType);
}
