namespace CoreBackend.Contracts.Auth.Responses;

public class SessionHistoryResponse
{
	public Guid Id { get; set; }
	public Guid? UserId { get; set; }
	public string? UserEmail { get; set; }
	public string? UserFullName { get; set; }
	public string SessionId { get; set; } = null!;
	public string Action { get; set; } = null!;
	public string? IpAddress { get; set; }
	public string? BrowserName { get; set; }
	public string? OperatingSystem { get; set; }
	public string? DeviceType { get; set; }
	public string? Country { get; set; }
	public string? City { get; set; }
	public DateTime CreatedAt { get; set; }
	public string? RevokedByUserName { get; set; }
	public string? RevokeReason { get; set; }
}