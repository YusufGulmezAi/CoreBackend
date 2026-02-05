using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;
using CoreBackend.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBackend.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	private const string DatabaseName = "IntegrationTestDb";

	public const string TestUserEmail = "admin@corebackend.com";
	public const string TestUserPassword = "Admin123!@#";
	public static Guid TestTenantId { get; private set; }
	public static Guid TestUserId { get; private set; }

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((context, config) =>
		{
			config.Sources.Clear();
			config.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["JwtSettings:SecretKey"] = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456789!",
				["JwtSettings:Issuer"] = "CoreBackend.Test",
				["JwtSettings:Audience"] = "CoreBackend.Test.Client",
				["JwtSettings:AccessTokenExpirationMinutes"] = "60",
				["JwtSettings:RefreshTokenExpirationDays"] = "7",
				["SuperAdmin:Email"] = TestUserEmail,
				["SuperAdmin:Password"] = TestUserPassword,
				["SuperAdmin:FirstName"] = "Super",
				["SuperAdmin:LastName"] = "Admin",
				["ConnectionStrings:DefaultConnection"] = "InMemory"
			});
		});

		builder.ConfigureServices(services =>
		{
			// Mevcut DbContext kayıtlarını kaldır
			var descriptorsToRemove = services
				.Where(d =>
					d.ServiceType.FullName?.Contains("DbContext") == true ||
					d.ServiceType.FullName?.Contains("EntityFramework") == true ||
					d.ServiceType.FullName?.Contains("Npgsql") == true ||
					d.ServiceType.FullName?.Contains("PostgreSql") == true ||
					d.ImplementationType?.FullName?.Contains("Npgsql") == true ||
					d.ImplementationType?.FullName?.Contains("PostgreSql") == true ||
					d.ImplementationType?.FullName?.Contains("DatabaseSeeder") == true)
				.ToList();

			foreach (var descriptor in descriptorsToRemove)
				services.Remove(descriptor);

			// InMemory database
			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseInMemoryDatabase(DatabaseName);
			});

			// Seed işlemini burada yap - IPasswordHasher kullanarak
			var sp = services.BuildServiceProvider();
			using var scope = sp.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

			context.Database.EnsureCreated();
			SeedTestData(context, passwordHasher);
		});

		builder.UseEnvironment("Testing");
	}

	private static void SeedTestData(ApplicationDbContext context, IPasswordHasher passwordHasher)
	{
		// Zaten seed edilmişse çık
		if (context.Users.Any(u => u.Email == TestUserEmail))
			return;

		// Tenant yoksa oluştur
		var tenant = context.Tenants.FirstOrDefault();
		if (tenant == null)
		{
			tenant = Tenant.Create("Test Tenant", "test@tenant.com", "+905551234567", 10, 60);
			context.Tenants.Add(tenant);
			context.SaveChanges();
		}
		TestTenantId = tenant.Id;

		// User oluştur - IPasswordHasher kullanarak
		var passwordHash = passwordHasher.HashPassword(TestUserPassword);
		var user = User.Create(tenant.Id, "admin", TestUserEmail, passwordHash, "Super", "Admin", "+905551234567");
		user.Activate();
		user.ConfirmEmail();
		context.Users.Add(user);
		context.SaveChanges();
		TestUserId = user.Id;

		// Role oluştur
		var adminRole = context.Roles.FirstOrDefault(r => r.Code == "ADMIN");
		if (adminRole == null)
		{
			adminRole = Role.Create(tenant.Id, "Admin", "ADMIN", RoleLevel.Tenant, "Administrator role", true);
			context.Roles.Add(adminRole);
			context.SaveChanges();
		}

		// UserRole oluştur
		if (!context.UserRoles.Any(ur => ur.UserId == user.Id))
		{
			var userRole = UserRole.Create(tenant.Id, user.Id, adminRole.Id);
			context.UserRoles.Add(userRole);
			context.SaveChanges();
		}
	}
}