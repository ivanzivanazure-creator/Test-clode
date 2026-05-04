namespace AccountingERP.Infrastructure;

using AccountingERP.Application.Queries.Journal;
using AccountingERP.Domain.Interfaces;
using AccountingERP.Infrastructure.Persistence;
using AccountingERP.Infrastructure.Repositories;
using AccountingERP.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all Infrastructure-layer services: EF Core, repositories,
    /// UnitOfWork, HashService and EncryptionService.
    /// Call this from the API host's Program.cs after AddApplication().
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        // ── Database ─────────────────────────────────────────────────────────

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    // Retry transient SQL Server errors (network blips, failover).
                    sql.EnableRetryOnFailure(
                        maxRetryCount:       5,
                        maxRetryDelay:       TimeSpan.FromSeconds(30),
                        errorNumbersToAdd:   null);
                });

            // Only enable sensitive logging in development — never in production.
#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // ── Repositories ──────────────────────────────────────────────────────

        // Scoped lifetime: one instance per HTTP request / unit of work scope.
        services.AddScoped<IInvoiceRepository,  InvoiceRepository>();
        // JournalRepository implements both IJournalRepository and IJournalEntriesReader.
        // Register it once and expose both interfaces from the same scoped instance.
        services.AddScoped<JournalRepository>();
        services.AddScoped<IJournalRepository>(sp  => sp.GetRequiredService<JournalRepository>());
        services.AddScoped<IJournalEntriesReader>(sp => sp.GetRequiredService<JournalRepository>());
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        // ── Unit of Work ──────────────────────────────────────────────────────

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Domain services ───────────────────────────────────────────────────

        // HashService is stateless; Singleton avoids repeated allocations.
        services.AddSingleton<IHashService, HashService>();

        // EncryptionService holds the AES key in memory; Singleton is appropriate
        // because the key is immutable after startup.
        services.AddSingleton<IEncryptionService, EncryptionService>();

        return services;
    }
}
