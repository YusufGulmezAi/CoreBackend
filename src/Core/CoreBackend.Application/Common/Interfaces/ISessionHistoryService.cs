using CoreBackend.Domain.Enums;

namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Session history servis interface.
/// </summary>
public interface ISessionHistoryService
{
	/// <summary>
	/// Session aktivitesi kaydeder.
	/// </summary>
	Task LogAsync(
		Guid tenantId,
		Guid userId,
		string sessionId,
		SessionAction action,
		DeviceInfo? deviceInfo = null,
		Guid? revokedByUserId = null,
		string? revokeReason = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Kullanıcının session geçmişini getirir.
	/// </summary>
	Task<IReadOnlyList<SessionHistoryDto>> GetUserHistoryAsync(
		Guid userId,
		int pageNumber = 1,
		int pageSize = 20,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Tenant'ın session geçmişini getirir.
	/// </summary>
	Task<IReadOnlyList<SessionHistoryDto>> GetTenantHistoryAsync(
		Guid tenantId,
		int pageNumber = 1,
		int pageSize = 50,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// Session history DTO.
/// </summary>
public class SessionHistoryDto
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public string? UserEmail { get; set; }
	public string? UserFullName { get; set; }
	public string SessionId { get; set; } = null!;
	public SessionAction Action { get; set; }
	public string ActionName => Action.ToString();
	public string? IpAddress { get; set; }
	public string? BrowserName { get; set; }
	public string? OperatingSystem { get; set; }
	public string? DeviceType { get; set; }
	public string? Country { get; set; }
	public string? City { get; set; }
	public DateTime CreatedAt { get; set; }
	public Guid? RevokedByUserId { get; set; }
	public string? RevokedByUserName { get; set; }
	public string? RevokeReason { get; set; }
}