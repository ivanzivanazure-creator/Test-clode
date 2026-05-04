namespace AccountingERP.Application.Queries.Invoices;

using AccountingERP.Application.Common;
using AccountingERP.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns the raw PDF bytes for an invoice.
/// Company name and PIB are read from application configuration (Company:Name, Company:PIB).
/// </summary>
public sealed record GetInvoicePdfQuery(int InvoiceId) : IRequest<Result<byte[]>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetInvoicePdfQueryHandler
    : IRequestHandler<GetInvoicePdfQuery, Result<byte[]>>
{
    private readonly IUnitOfWork        _uow;
    private readonly IInvoicePdfService _pdfService;
    private readonly IConfiguration     _config;

    public GetInvoicePdfQueryHandler(
        IUnitOfWork        uow,
        IInvoicePdfService pdfService,
        IConfiguration     config)
    {
        _uow        = uow;
        _pdfService = pdfService;
        _config     = config;
    }

    public async Task<Result<byte[]>> Handle(
        GetInvoicePdfQuery query,
        CancellationToken  cancellationToken)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(query.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result<byte[]>.Failure(
                $"Faktura sa ID={query.InvoiceId} nije pronađena.");

        var companyName = _config["Company:Name"] ?? "Naziv kompanije";
        var companyPIB  = _config["Company:PIB"]  ?? "000000000";

        var bytes = _pdfService.Generate(invoice, companyName, companyPIB);
        return Result<byte[]>.Success(bytes);
    }
}
