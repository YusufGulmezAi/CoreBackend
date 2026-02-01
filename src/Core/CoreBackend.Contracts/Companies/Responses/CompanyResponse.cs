namespace CoreBackend.Contracts.Companies.Responses;

public class CompanyResponse
{
	public Guid Id { get; set; }
	public Guid TenantId { get; set; }
	public string Name { get; set; } = null!;
	public string Code { get; set; } = null!;
	public string? TaxNumber { get; set; }
	public string? Address { get; set; }
	public string? Phone { get; set; }
	public string? Email { get; set; }
	public string Status { get; set; } = null!;
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}