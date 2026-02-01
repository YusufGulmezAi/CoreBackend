namespace CoreBackend.Contracts.Auth.Responses;

/// <summary>
/// Register response.
/// Kayıt sonrası döner.
/// </summary>
public class RegisterResponse
{
	/// <summary>
	/// Oluşturulan Tenant Id.
	/// </summary>
	public Guid TenantId { get; set; }

	/// <summary>
	/// Oluşturulan User Id.
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Email adresi.
	/// </summary>
	public string Email { get; set; } = null!;

	/// <summary>
	/// Kayıt başarılı mesajı.
	/// </summary>
	public string Message { get; set; } = null!;

	/// <summary>
	/// Email doğrulama gerekiyor mu?
	/// </summary>
	public bool RequiresEmailConfirmation { get; set; }
}