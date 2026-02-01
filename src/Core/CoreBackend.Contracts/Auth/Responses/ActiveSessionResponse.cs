namespace CoreBackend.Contracts.Auth.Responses;

public class ActiveSessionResponse
{
	public string SessionId { get; set; } = null!;
	public string IpAddress { get; set; } = null!;
	public string? BrowserName { get; set; }
	public string? OperatingSystem { get; set; }
	public string? DeviceType { get; set; }
	public string? Country { get; set; }
	public string? City { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime LastActivityAt { get; set; }
	public DateTime ExpiresAt { get; set; }
	public bool IsCurrent { get; set; }
}