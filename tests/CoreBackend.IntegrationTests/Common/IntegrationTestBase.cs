using CoreBackend.Contracts.Auth.Requests;
using CoreBackend.Contracts.Common;
using CoreBackend.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CoreBackend.IntegrationTests.Common;

/// <summary>
/// Tüm integration testler için base class.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
	protected readonly CustomWebApplicationFactory Factory;
	protected readonly HttpClient Client;
	private string? _cachedToken;

	protected IntegrationTestBase(CustomWebApplicationFactory factory)
	{
		Factory = factory;
		Client = factory.CreateClient();
	}

	#region Authentication Helpers

	protected async Task AuthenticateAsync()
	{
		if (!string.IsNullOrEmpty(_cachedToken))
		{
			Client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Bearer", _cachedToken);
			return;
		}

		var token = await GetAuthTokenAsync();
		if (!string.IsNullOrEmpty(token))
		{
			_cachedToken = token;
			Client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Bearer", token);
		}
	}

	protected async Task<string> GetAuthTokenAsync()
	{
		var loginRequest = new LoginRequest
		{
			Email = CustomWebApplicationFactory.TestUserEmail,
			Password = CustomWebApplicationFactory.TestUserPassword,
			RememberMe = false
		};

		var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

		// Debug: Response içeriğini yazdır
		var content = await response.Content.ReadAsStringAsync();

		if (!response.IsSuccessStatusCode)
		{
			// Login başarısız - hata detayını görmek için
			Console.WriteLine($"Login failed: {response.StatusCode}");
			Console.WriteLine($"Response: {content}");
			return string.Empty;
		}

		try
		{
			using var doc = JsonDocument.Parse(content);
			var root = doc.RootElement;

			if (root.TryGetProperty("data", out var data) &&
				data.TryGetProperty("accessToken", out var accessToken))
			{
				return accessToken.GetString() ?? string.Empty;
			}
		}
		catch (JsonException ex)
		{
			Console.WriteLine($"JSON parse error: {ex.Message}");
			Console.WriteLine($"Content: {content}");
		}

		return string.Empty;
	}

	#endregion

	#region Database Helpers

	protected async Task ExecuteDbContextAsync(Func<ApplicationDbContext, Task> action)
	{
		using var scope = Factory.Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await action(context);
	}

	protected async Task<T> ExecuteDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> func)
	{
		using var scope = Factory.Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		return await func(context);
	}

	protected async Task SeedAsync<T>(T entity) where T : class
	{
		await ExecuteDbContextAsync(async context =>
		{
			context.Set<T>().Add(entity);
			await context.SaveChangesAsync();
		});
	}

	#endregion
}