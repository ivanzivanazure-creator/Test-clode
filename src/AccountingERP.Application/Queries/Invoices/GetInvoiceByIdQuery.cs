namespace AccountingERP.Application.Queries.Invoices;

using AccountingERP.Application.Common;
using AccountingERP.Application.DTOs;
using AccountingERP.Domain.Interfaces;
using MediatR;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns a full invoice detail DTO (including all line items) for the given ID.
/// Returns a failure result when the invoice is not found.
/// </summary>
public sealed record GetInvoiceByIdQuery(int InvoiceId) : IRequest<Result<InvoiceDetailDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetInvoiceByIdQueryHandler
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDetailDto>>
{
    private readonly IUnitOfWork _uow;

    public GetInvoiceByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<InvoiceDetailDto>> Handle(
        GetInvoiceByIdQuery query,
        CancellationToken   cancellationToken)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(query.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result<InvoiceDetailDto>.Failure(
                $"Faktura sa ID={query.InvoiceId} nije pronađena.");

        return Result<InvoiceDetailDto>.Success(InvoiceDetailDto.FromDomain(invoice));
    }
}
