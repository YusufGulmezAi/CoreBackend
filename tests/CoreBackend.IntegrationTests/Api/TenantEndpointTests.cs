using System.Net;
using System.Net.Http.Json;
using CoreBackend.Contracts.Common;
using CoreBackend.Contracts.Tenants.Requests;
using CoreBackend.Contracts.Tenants.Responses;
using CoreBackend.IntegrationTests.Common;
using FluentAssertions;
using Xunit;

namespace CoreBackend.IntegrationTests.Api;

/// <summary>
/// Tenant endpoint integration testleri.
/// </summary>
public class TenantEndpointTests : IntegrationTestBase
{
	private const string BaseUrl = "/api/v1/tenants";

	public TenantEndpointTests(CustomWebApplicationFactory factory)
		: base(factory)
	{
	}

	#region Authorization Tests

	[Fact]
	public async Task GetTenants_WithoutAuth_ShouldReturn401()
	{
		// Act
		var response = await Client.GetAsync($"{BaseUrl}?pageNumber=1&pageSize=10");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	#endregion

	#region Create Tenant Tests

	[Fact]
	public async Task CreateTenant_WithValidData_ShouldReturnSuccess()
	{
		// Arrange
		await AuthenticateAsync();

		var request = new CreateTenantRequest
		{
			Name = "Integration Test Tenant",
			Email = $"test-{Guid.NewGuid()}@tenant.com",
			Phone = "+905551234567",
			MaxCompanyCount = 5
		};

		// Act
		var response = await Client.PostAsJsonAsync(BaseUrl, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var result = await response.Content.ReadFromJsonAsync<ApiResponse<TenantResponse>>();
		result.Should().NotBeNull();
		result!.Success.Should().BeTrue();
		result.Data.Should().NotBeNull();
		result.Data!.Name.Should().Be(request.Name);
	}

	[Fact]
	public async Task CreateTenant_WithInvalidEmail_ShouldReturnBadRequest()
	{
		// Arrange
		await AuthenticateAsync();

		var request = new CreateTenantRequest
		{
			Name = "Test Tenant",
			Email = "invalid-email",
			MaxCompanyCount = 5
		};

		// Act
		var response = await Client.PostAsJsonAsync(BaseUrl, request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	#endregion

	#region Get Tenant Tests

	[Fact]
	public async Task GetTenants_WithAuth_ShouldReturnOk()
	{
		// Arrange
		await AuthenticateAsync();

		// Act
		var response = await Client.GetAsync($"{BaseUrl}?pageNumber=1&pageSize=10");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task GetTenantById_WithInvalidId_ShouldReturnNotFound()
	{
		// Arrange
		await AuthenticateAsync();
		var invalidId = Guid.NewGuid();

		// Act
		var response = await Client.GetAsync($"{BaseUrl}/{invalidId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	#endregion

	#region Delete Tenant Tests

	[Fact]
	public async Task DeleteTenant_WithValidId_ShouldReturnOk()
	{
		// Arrange
		await AuthenticateAsync();

		// Önce bir tenant oluştur
		var createRequest = new CreateTenantRequest
		{
			Name = "To Delete",
			Email = $"delete-{Guid.NewGuid()}@tenant.com",
			MaxCompanyCount = 5
		};

		var createResponse = await Client.PostAsJsonAsync(BaseUrl, createRequest);

		// Create response'u kontrol et
		createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var content = await createResponse.Content.ReadAsStringAsync();
		content.Should().NotBeNullOrEmpty();

		var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TenantResponse>>();
		createResult.Should().NotBeNull();
		createResult!.Data.Should().NotBeNull();

		var tenantId = createResult.Data!.Id;

		// Act
		var response = await Client.DeleteAsync($"{BaseUrl}/{tenantId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task DeleteTenant_WithInvalidId_ShouldReturnBadRequestOrNotFound()
	{
		// Arrange
		await AuthenticateAsync();
		var invalidId = Guid.NewGuid();

		// Act
		var response = await Client.DeleteAsync($"{BaseUrl}/{invalidId}");

		// Assert
		response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
	}

	#endregion
}