namespace AccountingERP.Application;

using AccountingERP.Application.Common;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

/// <summary>
/// Extension methods for registering Application-layer services in the DI container.
/// Call <see cref="AddApplication"/> from the Infrastructure/API project's
/// <c>Program.cs</c> or <c>Startup.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR handlers, FluentValidation validators and the
    /// <see cref="ValidationBehavior{TRequest,TResponse}"/> pipeline behavior
    /// from the <c>AccountingERP.Application</c> assembly.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // ── MediatR ───────────────────────────────────────────────────────────
        // Scans the assembly for all IRequestHandler<,> implementations and
        // registers them with a scoped lifetime.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Register the validation pipeline so that every command/query is
            // automatically validated before the handler executes.
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // ── FluentValidation ──────────────────────────────────────────────────
        // Scans the same assembly for all AbstractValidator<T> implementations
        // and registers them as IValidator<T> with a transient lifetime.
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
