using CoreBackend.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CoreBackend.Application.Common.Settings;

/// <summary>
/// JWT servis implementasyonu.
/// Token oluşturma ve doğrulama işlemleri.
/// </summary>
public class JwtService : IJwtService
{
	private readonly JwtSettings _jwtSettings;
	private readonly SymmetricSecurityKey _securityKey;

	public JwtService(IOptions<JwtSettings> jwtSettings)
	{
		_jwtSettings = jwtSettings.Value;
		_securityKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
	}

	/// <summary>
	/// Access token oluşturur.
	/// </summary>
	public string GenerateAccessToken(JwtUserData userData)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, userData.UserId.ToString()),
			new(ClaimTypes.Email, userData.Email),
			new("tenant_id", userData.TenantId.ToString()),
			new("session_id", userData.SessionId),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var credentials = new SigningCredentials(
			_securityKey,
			SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _jwtSettings.Issuer,
			audience: _jwtSettings.Audience,
			claims: claims,
			expires: GetAccessTokenExpiration(),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	/// <summary>
	/// Refresh token oluşturur.
	/// </summary>
	public string GenerateRefreshToken()
	{
		var randomBytes = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomBytes);
		return Convert.ToBase64String(randomBytes);
	}

	/// <summary>
	/// Token'ı doğrular ve claim'leri döner.
	/// </summary>
	public JwtUserData? ValidateToken(string token)
	{
		try
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var validationParameters = GetValidationParameters();

			var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
			return ExtractUserData(principal);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Token'dan user data çıkarır (süre dolmuş olsa bile).
	/// </summary>
	public JwtUserData? GetUserDataFromExpiredToken(string token)
	{
		try
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var validationParameters = GetValidationParameters();
			validationParameters.ValidateLifetime = false; // Süre kontrolü yapma

			var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
			return ExtractUserData(principal);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Access token bitiş süresini döner.
	/// </summary>
	public DateTime GetAccessTokenExpiration()
	{
		return DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
	}

	/// <summary>
	/// Refresh token bitiş süresini döner.
	/// </summary>
	public DateTime GetRefreshTokenExpiration()
	{
		return DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
	}

	/// <summary>
	/// Token validation parametrelerini döner.
	/// </summary>
	private TokenValidationParameters GetValidationParameters()
	{
		return new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = _jwtSettings.Issuer,
			ValidAudience = _jwtSettings.Audience,
			IssuerSigningKey = _securityKey,
			ClockSkew = TimeSpan.Zero
		};
	}

	/// <summary>
	/// ClaimsPrincipal'dan JwtUserData çıkarır.
	/// </summary>
	private static JwtUserData? ExtractUserData(ClaimsPrincipal principal)
	{
		var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		var tenantIdClaim = principal.FindFirst("tenant_id")?.Value;
		var sessionIdClaim = principal.FindFirst("session_id")?.Value;
		var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;

		if (string.IsNullOrEmpty(userIdClaim) ||
			string.IsNullOrEmpty(tenantIdClaim) ||
			string.IsNullOrEmpty(sessionIdClaim) ||
			string.IsNullOrEmpty(emailClaim))
		{
			return null;
		}

		return new JwtUserData
		{
			UserId = Guid.Parse(userIdClaim),
			TenantId = Guid.Parse(tenantIdClaim),
			SessionId = sessionIdClaim,
			Email = emailClaim
		};
	}
}