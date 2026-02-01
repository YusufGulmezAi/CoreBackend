namespace CoreBackend.Contracts.Companies.Requests;

public class UpdateCompanyRequest
{
	public string Name { get; set; } = null!;
	public string? TaxNumber { get; set; }
	public string? Address { get; set; }
	public string? Phone { get; set; }
	public string? Email { get; set; }
}