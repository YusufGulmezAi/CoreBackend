using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Infrastructure.Persistence;
using CoreBackend.Infrastructure.Persistence.Context;
using CoreBackend.Infrastructure.Persistence.Repositories;
using CoreBackend.Infrastructure.Services;
using CoreBackend.Infrastructure.Services.Caching;
using CoreBackend.Infrastructure.Services.Localization;

namespace CoreBackend.Infrastructure;

/// <summary>
/// Infrastructure katmanı dependency injection kayıtları.
/// </summary>
public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// Database
		services.AddDatabase(configuration);

		// Redis Cache
		services.AddRedisCache(configuration);

		// Repositories
		services.AddRepositories();

		// Services
		services.AddServices();

		return services;
	}

	/// <summary>
	/// Veritabanı yapılandırması.
	/// </summary>
	private static IServiceCollection AddDatabase(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var databaseProvider = configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";
		var connectionString = configuration.GetConnectionString("DefaultConnection");

		services.AddDbContext<ApplicationDbContext>((sp, options) =>
		{
			switch (databaseProvider.ToLower())
			{
				case "postgresql":
				case "postgres":
					options.UseNpgsql(connectionString, npgsqlOptions =>
					{
						npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
						npgsqlOptions.EnableRetryOnFailure(3);
					});
					break;

				case "sqlserver":
				case "mssql":
					options.UseSqlServer(connectionString, sqlOptions =>
					{
						sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
						sqlOptions.EnableRetryOnFailure(3);
					});
					break;

				default:
					throw new InvalidOperationException($"Unsupported database provider: {databaseProvider}");
			}

			// Development ortamında detaylı loglama
#if DEBUG
			options.EnableSensitiveDataLogging();
			options.EnableDetailedErrors();
#endif
		});

		return services;
	}

	/// <summary>
	/// Redis cache yapılandırması.
	/// </summary>
	private static IServiceCollection AddRedisCache(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

		// Redis connection multiplexer
		services.AddSingleton<IConnectionMultiplexer>(sp =>
		{
			var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
			configurationOptions.AbortOnConnectFail = false;
			return ConnectionMultiplexer.Connect(configurationOptions);
		});

		// Distributed cache
		services.AddStackExchangeRedisCache(options =>
		{
			options.Configuration = redisConnectionString;
			options.InstanceName = "CoreBackend:";
		});

		return services;
	}

	/// <summary>
	/// Repository kayıtları.
	/// </summary>
	private static IServiceCollection AddRepositories(this IServiceCollection services)
	{
		// Generic repository
		services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
		services.AddScoped(typeof(IRepositoryExtended<,>), typeof(RepositoryExtended<,>));

		// Unit of Work
		services.AddScoped<IUnitOfWork, UnitOfWork>();

		return services;
	}

	/// <summary>
	/// Servis kayıtları.
	/// </summary>
	private static IServiceCollection AddServices(this IServiceCollection services)
	{
		// Http Context Accessor
		services.AddHttpContextAccessor();

		// Tenant & User Services
		services.AddScoped<ITenantService, TenantService>();
		services.AddScoped<ICurrentUserService, CurrentUserService>();

		// Cache Service
		services.AddScoped<ICacheService, RedisCacheService>();

		// Session Service
		services.AddScoped<IUserSessionService, UserSessionService>();

		// Localization Service
		services.AddSingleton<ILocalizationService, LocalizationService>();

		return services;
	}
}