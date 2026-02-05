using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreBackend.Infrastructure.Persistence.Seeding;

/// <summary>
/// Uygulama başlatıldığında database seeding işlemini çalıştırır.
/// </summary>
public class DatabaseSeederHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseSeederHostedService> _logger;

    public DatabaseSeederHostedService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseSeederHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== DATABASE SEEDING STARTED ===");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

            await seeder.SeedAsync(cancellationToken);

            _logger.LogInformation("=== DATABASE SEEDING COMPLETED ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== DATABASE SEEDING FAILED === Error: {Message}", ex.Message);
            
            // Inner exception'ı da logla
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner Exception: {InnerMessage}", ex.InnerException.Message);
            }
            
            // Uygulama başlamasını engellemek istemiyorsak throw etmeyin
            // throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}