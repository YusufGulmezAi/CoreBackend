using Asp.Versioning;

namespace CoreBackend.Api.Endpoints.v1;

/// <summary>
/// Health check endpoint.
/// </summary>
public class HealthEndpoint : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		var versionSet = app.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1, 0))
			.Build();

		app.MapGet("/api/v{version:apiVersion}/health", GetHealth)
			.WithApiVersionSet(versionSet)
			.WithName("Health")
			.WithTags("Health")
			.AllowAnonymous();
	}

	private static IResult GetHealth()
	{
		var response = new
		{
			Status = "Healthy",
			Timestamp = DateTime.UtcNow,
			Version = "1.0.0",
			Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
		};

		return Results.Ok(response);
	}
}