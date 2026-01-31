using System.Reflection;
using CoreBackend.Api.Endpoints;

namespace CoreBackend.Api.Extensions;

/// <summary>
/// Endpoint extension metodları.
/// </summary>
public static class EndpointExtensions
{
	/// <summary>
	/// Tüm endpoint'leri otomatik keşfeder ve kaydeder.
	/// </summary>
	public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
	{
		var endpointTypes = assembly
			.GetTypes()
			.Where(t => t.IsClass && !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t));

		foreach (var type in endpointTypes)
		{
			services.AddScoped(typeof(IEndpoint), type);
		}

		return services;
	}

	/// <summary>
	/// Tüm endpoint'leri route'a ekler.
	/// </summary>
	public static IApplicationBuilder MapEndpoints(this WebApplication app)
	{
		using var scope = app.Services.CreateScope();
		var endpoints = scope.ServiceProvider.GetServices<IEndpoint>();

		foreach (var endpoint in endpoints)
		{
			endpoint.MapEndpoint(app);
		}

		return app;
	}
}