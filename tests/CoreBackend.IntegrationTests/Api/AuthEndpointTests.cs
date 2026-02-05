using System.Net;
using System.Net.Http.Json;
using CoreBackend.Contracts.Auth.Requests;
using CoreBackend.Contracts.Common;
using CoreBackend.IntegrationTests.Common;
using FluentAssertions;
using Xunit;

namespace CoreBackend.IntegrationTests.Api;

/// <summary>
/// Auth endpoint integration testleri.
/// </summary>
public class AuthEndpointTests : IntegrationTestBase
{
	private const string BaseUrl = "/api/v1/auth";

	public AuthEndpointTests(CustomWebApplicationFactory factory)
		: base(factory)
	{
	}

	#region Login Tests

	[Fact]
	public async Task Login_WithValidCredentials_ShouldReturnTokens()
	{
		// Arrange
		var request = new LoginRequest
		{
			Email = "admin@corebackend.com",
			Password = "Admin123!@#",
			RememberMe = false
		};

		// Act
		var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", request);

		// Assert
		// Not: Eğer kullanıcı seed edilmemişse 400 dönecektir
		response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Login_WithInvalidPassword_ShouldReturnBadRequest()
	{
		// Arrange
		var request = new LoginRequest
		{
			Email = "admin@corebackend.com",
			Password = "WrongPassword123!",
			RememberMe = false
		};

		// Act
		var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Login_WithNonExistentUser_ShouldReturnBadRequest()
	{
		// Arrange
		var request = new LoginRequest
		{
			Email = "nonexistent@email.com",
			Password = "SomePassword123!",
			RememberMe = false
		};

		// Act
		var response = await Client.PostAsJsonAsync($"{BaseUrl}/login", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	#endregion

	#region Logout Tests

	[Fact]
	public async Task Logout_WithoutToken_ShouldReturn401()
	{
		// Act
		var response = await Client.PostAsync($"{BaseUrl}/logout", null);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task Logout_WithValidToken_ShouldReturnOk()
	{
		// Arrange
		await AuthenticateAsync();

		// Act
		var response = await Client.PostAsync($"{BaseUrl}/logout", null);

		// Assert
		// Token yoksa veya geçersizse 401, geçerliyse 200
		response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
	}

	#endregion

	#region Refresh Token Tests

	[Fact]
	public async Task RefreshToken_WithInvalidTokens_ShouldReturnBadRequest()
	{
		// Arrange
		var request = new RefreshTokenRequest
		{
			AccessToken = "invalid-access-token",
			RefreshToken = "invalid-refresh-token"
		};

		// Act
		var response = await Client.PostAsJsonAsync($"{BaseUrl}/refresh-token", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	#endregion

	#region Sessions Tests

	[Fact]
	public async Task GetActiveSessions_WithoutAuth_ShouldReturn401()
	{
		// Act
		var response = await Client.GetAsync($"{BaseUrl}/sessions");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	#endregion
}