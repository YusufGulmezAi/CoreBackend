using CoreBackend.Domain.Enums;

namespace CoreBackend.Contracts.Tenants.Responses;

public class TenantResponse
{
	public Guid Id { get; set; }
	public string Name { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string? Phone { get; set; }
	public string? Subdomain { get; set; }
	public string? ContactEmail { get; set; }
	public string? ContactPhone { get; set; }
	public TenantStatus? Status { get; set; }
	public int MaxCompanyCount { get; set; }
	public int? SessionTimeoutMinutes { get; set; }
	public DateTime? SubscriptionStartDate { get; set; }
	public DateTime? SubscriptionEndDate { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public int CompanyCount { get; set; }
	public int UserCount { get; set; }
}