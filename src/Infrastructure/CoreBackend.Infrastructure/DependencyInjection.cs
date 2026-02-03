using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Settings;
using CoreBackend.Infrastructure.Persistence;
using CoreBackend.Infrastructure.Persistence.Context;
using CoreBackend.Infrastructure.Services;
using CoreBackend.Infrastructure.Services.Caching;
using CoreBackend.Infrastructure.Services.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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
		services.AddDatabase(configuration);
		services.AddRepositories();
		services.AddServices(configuration);
		services.AddCaching(configuration);

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
	/// Repository kayıtları.
	/// </summary>
	private static IServiceCollection AddRepositories(this IServiceCollection services)
	{		
		// Unit of Work (Hybrid yaklaşım)
		services.AddScoped<IUnitOfWork, UnitOfWork>();

		return services;
	}

	/// <summary>
	/// Servis kayıtları.
	/// </summary>
	private static IServiceCollection AddServices(this IServiceCollection services,
		IConfiguration configuration)
	{
		// Settings
		services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
		services.Configure<SessionSettings>(configuration.GetSection(SessionSettings.SectionName));
		services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
		services.Configure<NetgsmSettings>(configuration.GetSection(NetgsmSettings.SectionName));

		// Core Services
		services.AddScoped<ITenantService, TenantService>();
		services.AddScoped<ICurrentUserService, CurrentUserService>();
		services.AddScoped<IUserSessionService, UserSessionService>();
		services.AddScoped<ILocalizationService, LocalizationService>();

		// Auth Services
		services.AddScoped<IPasswordHasher, PasswordHasher>();
		services.AddScoped<IJwtService, JwtService>();
		services.AddScoped<IDeviceInfoService, DeviceInfoService>();
		services.AddScoped<ITotpService, TotpService>();

		// Session History Service
		services.AddScoped<ISessionHistoryService, SessionHistoryService>();

		// Email Service
		services.AddScoped<IEmailService, EmailService>();

		// SMS Service (HttpClient ile)
		services.AddHttpClient<ISmsService, NetgsmSmsService>(client =>
		{
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// Http Context Accessor
		services.AddHttpContextAccessor();

		return services;
	}


	/// <summary>
	/// Cache servislerini ekler.
	/// </summary>
	/// <summary>
	/// Cache servislerini ekler.
	/// </summary>
	private static IServiceCollection AddCaching(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var redisConnection = configuration.GetConnectionString("Redis");

		if (!string.IsNullOrEmpty(redisConnection))
		{
			services.AddStackExchangeRedisCache(options =>
			{
				options.Configuration = redisConnection;
				options.InstanceName = "CoreBackend:";
			});

			services.AddScoped<ICacheService, RedisCacheService>();
		}
		else
		{
			services.AddDistributedMemoryCache();
			services.AddScoped<ICacheService, RedisCacheService>();
		}

		return services;
	}


}