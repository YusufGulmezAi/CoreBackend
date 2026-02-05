namespace CoreBackend.Contracts.Tenants.Requests;

public class CreateTenantRequest
{
	public string Name { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string? Phone { get; set; }
	public string? Subdomain { get; set; }
	public int MaxCompanyCount { get; set; } = 5;
	public int? SessionTimeoutMinutes { get; set; }
}