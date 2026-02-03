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
		_logger.LogInformation("Starting database seeding...");

		using var scope = _serviceProvider.CreateScope();
		var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

		await seeder.SeedAsync(cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}