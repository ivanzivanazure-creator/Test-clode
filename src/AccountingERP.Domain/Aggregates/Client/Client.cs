namespace AccountingERP.Domain.Aggregates.Client;

using AccountingERP.Domain.Common;
using AccountingERP.Domain.Events;
using AccountingERP.Domain.Exceptions;

public enum ClientType { Kupac, Dobavljac, Oba }

public class Client : AggregateRoot<int>
{
    public int        TenantId     { get; private set; }
    public string     Name         { get; private set; } = null!;
    public string?    PIB          { get; private set; }
    public string?    MaticniBroj  { get; private set; }
    public string?    Address      { get; private set; }
    public string?    City         { get; private set; }
    public string     Country      { get; private set; } = "RS";
    public string?    IBAN         { get; private set; }
    public string?    Email        { get; private set; }
    public string?    Phone        { get; private set; }
    public bool       IsActive     { get; private set; }
    public ClientType ClientType   { get; private set; }

    private Client() { }

    public static Client Create(int tenantId, string name, ClientType clientType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Naziv klijenta je obavezan.");

        var client = new Client
        {
            TenantId   = tenantId,
            Name       = name.Trim(),
            ClientType = clientType,
            IsActive   = true,
            Country    = "RS",
        };

        client.Raise(new ClientCreatedEvent(tenantId, client.Name, clientType));
        return client;
    }

    public void Update(
        string  name,
        string? pib,
        string? maticniBroj,
        string? address,
        string? city,
        string? iban,
        string? email,
        string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Naziv klijenta je obavezan.");

        Name        = name.Trim();
        PIB         = pib;
        MaticniBroj = maticniBroj;
        Address     = address;
        City        = city;
        IBAN        = iban;
        Email       = email;
        Phone       = phone;

        Raise(new ClientUpdatedEvent(TenantId, Id, Name));
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
