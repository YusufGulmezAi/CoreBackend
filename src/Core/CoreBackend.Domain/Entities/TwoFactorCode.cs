using CoreBackend.Domain.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Domain.Entities;

/// <summary>
/// 2FA doğrulama kodu entity.
/// Email ve SMS kodları için kullanılır.
/// </summary>
public class TwoFactorCode : BaseEntity<Guid>, ITenantEntity
{
	public Guid TenantId { get; private set; }
	public Guid UserId { get; private set; }
	public string Code { get; private set; } = null!;
	public TwoFactorMethod Method { get; private set; }
	public DateTime ExpiresAt { get; private set; }
	public bool IsUsed { get; private set; }
	public DateTime? UsedAt { get; private set; }
	public int AttemptCount { get; private set; }
	public DateTime CreatedAt { get; private set; }

	// Navigation
	public virtual User User { get; private set; } = null!;
	public virtual Tenant Tenant { get; private set; } = null!;

	private TwoFactorCode() { }

	private TwoFactorCode(
		Guid id,
		Guid tenantId,
		Guid userId,
		string code,
		TwoFactorMethod method,
		int expirationMinutes) : base(id)
	{
		TenantId = tenantId;
		UserId = userId;
		Code = code;
		Method = method;
		ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
		IsUsed = false;
		AttemptCount = 0;
		CreatedAt = DateTime.UtcNow;
	}

	/// <summary>
	/// Yeni 2FA kodu oluşturur.
	/// </summary>
	public static TwoFactorCode Create(
		Guid tenantId,
		Guid userId,
		TwoFactorMethod method,
		int expirationMinutes = 5)
	{
		var code = GenerateCode();
		return new TwoFactorCode(
			Guid.NewGuid(),
			tenantId,
			userId,
			code,
			method,
			expirationMinutes);
	}

	/// <summary>
	/// 6 haneli kod oluşturur.
	/// </summary>
	private static string GenerateCode()
	{
		var random = new Random();
		return random.Next(100000, 999999).ToString();
	}

	/// <summary>
	/// Kodu doğrular.
	/// </summary>
	public bool Verify(string inputCode)
	{
		AttemptCount++;

		if (IsUsed || DateTime.UtcNow > ExpiresAt || AttemptCount > 5)
		{
			return false;
		}

		if (Code == inputCode)
		{
			IsUsed = true;
			UsedAt = DateTime.UtcNow;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Kodun geçerli olup olmadığını kontrol eder.
	/// </summary>
	public bool IsValid()
	{
		return !IsUsed && DateTime.UtcNow <= ExpiresAt && AttemptCount <= 5;
	}
}