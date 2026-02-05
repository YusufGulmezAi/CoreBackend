using CoreBackend.Contracts.Auth.Requests;
using CoreBackend.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace CoreBackend.IntegrationTests.Api;

/// <summary>
/// Auth sisteminin debug testleri.
/// </summary>
public class AuthDebugTests : IntegrationTestBase
{
	private readonly ITestOutputHelper _output;

	public AuthDebugTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
		: base(factory)
	{
		_output = output;
	}

	[Fact]
	public async Task Debug_CheckTestUserExists()
	{
		// Veritabanında kullanıcı var mı kontrol et
		await ExecuteDbContextAsync(async context =>
		{
			var users = await context.Users.ToListAsync();
			_output.WriteLine($"Total users: {users.Count}");

			foreach (var user in users)
			{
				_output.WriteLine($"User: {user.Email}, Status: {user.Status}, EmailConfirmed: {user.EmailConfirmed}");
			}

			var testUser = users.FirstOrDefault(u => u.Email == CustomWebApplicationFactory.TestUserEmail);
			testUser.Should().NotBeNull($"Test user {CustomWebApplicationFactory.TestUserEmail} should exist");
		});
	}

	[Fact]
	public async Task Debug_CheckTenantExists()
	{
		await ExecuteDbContextAsync(async context =>
		{
			var tenants = await context.Tenants.ToListAsync();
			_output.WriteLine($"Total tenants: {tenants.Count}");

			foreach (var tenant in tenants)
			{
				_output.WriteLine($"Tenant: {tenant.Name}, Email: {tenant.Email}, Status: {tenant.Status}");
			}

			tenants.Count.Should().BeGreaterThan(0, "At least one tenant should exist");
		});
	}

	[Fact]
	public async Task Debug_LoginAttempt()
	{
		var loginRequest = new LoginRequest
		{
			Email = CustomWebApplicationFactory.TestUserEmail,
			Password = CustomWebApplicationFactory.TestUserPassword,
			RememberMe = false
		};

		_output.WriteLine($"Attempting login with: {loginRequest.Email}");

		var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
		var content = await response.Content.ReadAsStringAsync();

		_output.WriteLine($"Status: {response.StatusCode}");
		_output.WriteLine($"Response: {content}");

		// Sadece response'u göster, assertion yapma
	}

	[Fact]
	public async Task Debug_CheckUserRoles()
	{
		await ExecuteDbContextAsync(async context =>
		{
			var userRoles = await context.UserRoles
				.Include(ur => ur.User)
				.Include(ur => ur.Role)
				.ToListAsync();

			_output.WriteLine($"Total user roles: {userRoles.Count}");

			foreach (var ur in userRoles)
			{
				_output.WriteLine($"UserRole: User={ur.User?.Email}, Role={ur.Role?.Name}");
			}
		});
	}
}