using CoreBackend.Domain.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Domain.Entities;

/// <summary>
/// Session geçmişi entity.
/// Tüm session aktivitelerini kalıcı olarak saklar.
/// </summary>
public class SessionHistory : BaseEntity<Guid>, ITenantEntity
{
	public Guid TenantId { get; private set; }
	public Guid UserId { get; private set; }
	public string SessionId { get; private set; } = null!;
	public SessionAction Action { get; private set; }
	public string? IpAddress { get; private set; }
	public string? UserAgent { get; private set; }
	public string? BrowserName { get; private set; }
	public string? OperatingSystem { get; private set; }
	public string? DeviceType { get; private set; }
	public string? GeoLocation { get; private set; }
	public string? Country { get; private set; }
	public string? City { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public Guid? RevokedByUserId { get; private set; }
	public string? RevokeReason { get; private set; }
	public string? AdditionalData { get; private set; }

	// Navigation
	public virtual User User { get; private set; } = null!;
	public virtual User? RevokedByUser { get; private set; }
	public virtual Tenant Tenant { get; private set; } = null!;

	private SessionHistory() { }

	private SessionHistory(
		Guid id,
		Guid tenantId,
		Guid userId,
		string sessionId,
		SessionAction action,
		string? ipAddress,
		string? userAgent,
		string? browserName,
		string? operatingSystem,
		string? deviceType,
		string? geoLocation,
		string? country,
		string? city) : base(id)
	{
		TenantId = tenantId;
		UserId = userId;
		SessionId = sessionId;
		Action = action;
		IpAddress = ipAddress;
		UserAgent = userAgent;
		BrowserName = browserName;
		OperatingSystem = operatingSystem;
		DeviceType = deviceType;
		GeoLocation = geoLocation;
		Country = country;
		City = city;
		CreatedAt = DateTime.UtcNow;
	}

	/// <summary>
	/// Yeni session history kaydı oluşturur.
	/// </summary>
	public static SessionHistory Create(
		Guid tenantId,
		Guid userId,
		string sessionId,
		SessionAction action,
		string? ipAddress = null,
		string? userAgent = null,
		string? browserName = null,
		string? operatingSystem = null,
		string? deviceType = null,
		string? geoLocation = null,
		string? country = null,
		string? city = null)
	{
		return new SessionHistory(
			Guid.NewGuid(),
			tenantId,
			userId,
			sessionId,
			action,
			ipAddress,
			userAgent,
			browserName,
			operatingSystem,
			deviceType,
			geoLocation,
			country,
			city);
	}

	/// <summary>
	/// Admin tarafından iptal bilgisini ekler.
	/// </summary>
	public void SetRevokedBy(Guid revokedByUserId, string? reason = null)
	{
		RevokedByUserId = revokedByUserId;
		RevokeReason = reason;
	}

	/// <summary>
	/// Ek veri ekler (JSON formatında).
	/// </summary>
	public void SetAdditionalData(string data)
	{
		AdditionalData = data;
	}
}