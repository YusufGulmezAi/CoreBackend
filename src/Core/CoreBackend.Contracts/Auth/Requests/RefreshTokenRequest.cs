namespace CoreBackend.Contracts.Auth.Requests;

/// <summary>
/// Token yenileme isteği.
/// </summary>
public class RefreshTokenRequest
{
	/// <summary>
	/// Mevcut access token.
	/// </summary>
	public string AccessToken { get; set; } = null!;

	/// <summary>
	/// Refresh token.
	/// </summary>
	public string RefreshToken { get; set; } = null!;
}