namespace AccountingERP.Application.Common;

using FluentValidation;
using MediatR;

/// <summary>
/// MediatR pipeline behavior that runs all registered FluentValidation validators
/// for the request before the handler is invoked.
/// Returns a failed Result when validation errors are found; otherwise passes
/// the request through to the next delegate in the pipeline.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest                          request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken                 cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        // Build a human-readable message that groups errors by property.
        var message = string.Join("; ", failures.Select(f => f.ErrorMessage));

        // Result<T> supports implicit conversion from string → Failure.
        // We use reflection-free dynamic dispatch: if TResponse is Result<T>,
        // call Result<T>.Failure; otherwise throw a ValidationException.
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Invoke Result<T>.Failure(string) via the static factory.
            var failureMethod = responseType.GetMethod(
                nameof(Result<object>.Failure),
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                null,
                new[] { typeof(string) },
                null)!;

            return (TResponse)failureMethod.Invoke(null, new object[] { message })!;
        }

        // Fallback: throw so MediatR exception-handling middleware can catch it.
        throw new ValidationException(failures);
    }
}
