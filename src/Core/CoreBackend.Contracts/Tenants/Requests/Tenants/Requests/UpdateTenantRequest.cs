namespace CoreBackend.Contracts.Tenants.Requests;

public class UpdateTenantRequest
{
	public string Name { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string? Phone { get; set; }
	public string? Subdomain { get; set; }
	public string? ContactEmail { get; set; }
	public string? ContactPhone { get; set; }
	public int? MaxCompanyCount { get; set; }
	public int? SessionTimeoutMinutes { get; set; }
}