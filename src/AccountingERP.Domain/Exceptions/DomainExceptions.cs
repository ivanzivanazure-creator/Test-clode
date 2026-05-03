namespace AccountingERP.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) {}
}

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object id)
        : base($"{entity} sa ID={id} nije pronađen") {}
}

public class PeriodLockedException : DomainException
{
    public PeriodLockedException(int month, int year)
        : base($"Računovodstveni period {month:D2}/{year} je zaključan — retroaktivna knjiženja nisu dozvoljena") {}
}

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string productCode, decimal requested, decimal available)
        : base($"Artikal {productCode}: tražena količina {requested} > dostupna {available}") {}
}

public class BusinessException : Exception
{
    public string Code { get; }
    public BusinessException(string code, string message) : base(message) => Code = code;
}
