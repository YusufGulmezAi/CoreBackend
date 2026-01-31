namespace CoreBackend.Api.Endpoints;

/// <summary>
/// Minimal API endpoint interface.
/// Tüm endpoint'ler bu interface'i implemente eder.
/// </summary>
public interface IEndpoint
{
	/// <summary>
	/// Endpoint'i route'a ekler.
	/// </summary>
	void MapEndpoint(IEndpointRouteBuilder app);
}