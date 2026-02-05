using CoreBackend.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoreBackend.IntegrationTests.Api;

/// <summary>
/// InMemory veritabanının doğru şekilde seed edildiğini doğrular.
/// </summary>
public class DatabaseSeedTests : IntegrationTestBase
{
	public DatabaseSeedTests(CustomWebApplicationFactory factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task Database_ShouldBeCreated()
	{
		await ExecuteDbContextAsync(async context =>
		{
			var canConnect = await context.Database.CanConnectAsync();
			canConnect.Should().BeTrue("InMemory veritabanı oluşturulmuş olmalı");
		});
	}

	[Fact]
	public async Task Database_ShouldHaveTestTenant()
	{
		await ExecuteDbContextAsync(async context =>
		{
			var tenantCount = await context.Tenants.CountAsync();
			tenantCount.Should().BeGreaterThan(0, "En az bir test tenant seed edilmiş olmalı");
		});
	}

	[Fact]
	public async Task Database_AllDbSetsShouldBeAccessible()
	{
		await ExecuteDbContextAsync(async context =>
		{
			var tenants = await context.Tenants.ToListAsync();
			var users = await context.Users.ToListAsync();
			var roles = await context.Roles.ToListAsync();

			tenants.Should().NotBeNull();
			users.Should().NotBeNull();
			roles.Should().NotBeNull();
		});
	}
}