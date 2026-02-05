using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Settings;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;
using CoreBackend.Infrastructure.Persistence.Context;

namespace CoreBackend.Infrastructure.Persistence.Seeding;

/// <summary>
/// Veritabanı başlangıç verilerini oluşturur.
/// Super Admin, System Tenant, Roller ve Temel İzinler.
/// </summary>
public class DatabaseSeeder
{
	private readonly ApplicationDbContext _context;
	private readonly IPasswordHasher _passwordHasher;
	private readonly SuperAdminSettings _superAdminSettings;
	private readonly ILogger<DatabaseSeeder> _logger;

	// Sabit ID'ler (tutarlılık için)
	private static readonly Guid SystemTenantId = new("00000000-0000-0000-0000-000000000001");
	private static readonly Guid SuperAdminRoleId = new("00000000-0000-0000-0000-000000000001");
	private static readonly Guid TenantAdminRoleId = new("00000000-0000-0000-0000-000000000002");
	private static readonly Guid SuperAdminUserId = new("00000000-0000-0000-0000-000000000001");

	public DatabaseSeeder(
		ApplicationDbContext context,
		IPasswordHasher passwordHasher,
		IOptions<SuperAdminSettings> superAdminSettings,
		ILogger<DatabaseSeeder> logger)
	{
		_context = context;
		_passwordHasher = passwordHasher;
		_superAdminSettings = superAdminSettings.Value;
		_logger = logger;
	}

	/// <summary>
	/// Seed işlemini çalıştırır.
	/// </summary>
	public async Task SeedAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("=== DATABASE SEEDING STARTED ===");

			// Migration'ları uygula
			_logger.LogInformation("Applying migrations...");
			await _context.Database.MigrateAsync(cancellationToken);
			_logger.LogInformation("Migrations applied successfully");

			// Seed işlemleri
			_logger.LogInformation("Seeding permissions...");
			await SeedPermissionsAsync(cancellationToken);

			_logger.LogInformation("Seeding system tenant... Email: {Email}", _superAdminSettings.Email ?? "NULL");
			await SeedSystemTenantAsync(cancellationToken);

			_logger.LogInformation("Seeding system roles...");
			await SeedSystemRolesAsync(cancellationToken);

			_logger.LogInformation("Seeding super admin... Username: {Username}", _superAdminSettings.Username ?? "NULL");
			await SeedSuperAdminAsync(cancellationToken);

			_logger.LogInformation("=== DATABASE SEEDING COMPLETED ===");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "=== DATABASE SEEDING FAILED === Error: {Message}", ex.Message);
			throw;
		}
	}

	/// <summary>
	/// Temel izinleri oluşturur.
	/// </summary>
	private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
	{
		if (await _context.Permissions.AnyAsync(cancellationToken))
			return;

		var permissions = new List<Permission>
		{
            // Tenant yönetimi
            Permission.Create("Tenant Görüntüle", "tenants.view", "Tenant Management", "Tenant listesini görüntüleme"),
			Permission.Create("Tenant Oluştur", "tenants.create", "Tenant Management", "Yeni tenant oluşturma"),
			Permission.Create("Tenant Düzenle", "tenants.edit", "Tenant Management", "Tenant bilgilerini düzenleme"),
			Permission.Create("Tenant Sil", "tenants.delete", "Tenant Management", "Tenant silme"),

            // Kullanıcı yönetimi
            Permission.Create("Kullanıcı Görüntüle", "users.view", "User Management", "Kullanıcı listesini görüntüleme"),
			Permission.Create("Kullanıcı Oluştur", "users.create", "User Management", "Yeni kullanıcı oluşturma"),
			Permission.Create("Kullanıcı Düzenle", "users.edit", "User Management", "Kullanıcı bilgilerini düzenleme"),
			Permission.Create("Kullanıcı Sil", "users.delete", "User Management", "Kullanıcı silme"),

            // Rol yönetimi
            Permission.Create("Rol Görüntüle", "roles.view", "Role Management", "Rol listesini görüntüleme"),
			Permission.Create("Rol Oluştur", "roles.create", "Role Management", "Yeni rol oluşturma"),
			Permission.Create("Rol Düzenle", "roles.edit", "Role Management", "Rol bilgilerini düzenleme"),
			Permission.Create("Rol Sil", "roles.delete", "Role Management", "Rol silme"),

            // Şirket yönetimi
            Permission.Create("Şirket Görüntüle", "companies.view", "Company Management", "Şirket listesini görüntüleme"),
			Permission.Create("Şirket Oluştur", "companies.create", "Company Management", "Yeni şirket oluşturma"),
			Permission.Create("Şirket Düzenle", "companies.edit", "Company Management", "Şirket bilgilerini düzenleme"),
			Permission.Create("Şirket Sil", "companies.delete", "Company Management", "Şirket silme"),

            // Sistem yönetimi
            Permission.Create("Sistem Ayarları", "system.settings", "System", "Sistem ayarlarını yönetme"),
			Permission.Create("Audit Logları", "system.audit", "System", "Audit loglarını görüntüleme"),
			Permission.Create("Tüm Erişim", "system.full_access", "System", "Tüm sisteme tam erişim"),
		};

		await _context.Permissions.AddRangeAsync(permissions, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("Seeded {Count} permissions", permissions.Count);
	}

	/// <summary>
	/// Sistem tenant'ını oluşturur.
	/// </summary>
	/// <summary>
	/// Sistem tenant'ını oluşturur.
	/// </summary>
	private async Task SeedSystemTenantAsync(CancellationToken cancellationToken)
	{
		var exists = await _context.Tenants
			.IgnoreQueryFilters()
			.AnyAsync(t => t.Id == SystemTenantId, cancellationToken);

		if (exists)
		{
			_logger.LogInformation("System tenant already exists");
			return;
		}

		// Email null kontrolü
		var email = _superAdminSettings.Email;
		if (string.IsNullOrWhiteSpace(email))
		{
			email = "system@corebackend.local";
			_logger.LogWarning("SuperAdmin:Email is empty, using default: {Email}", email);
		}

		var systemTenant = Tenant.Create(
			"System",
			email,
			null,
			maxCompanyCount: int.MaxValue,
			sessionTimeoutMinutes: 60);

		// Private ID setter için reflection kullan
		typeof(Tenant).GetProperty("Id")!
			.SetValue(systemTenant, SystemTenantId);

		// Zorunlu alanları set et
		systemTenant.Subdomain = "system";
		systemTenant.ContactEmail = email;
		systemTenant.ContactPhone = "";

		await _context.Tenants.AddAsync(systemTenant, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("System tenant created with ID: {TenantId}", SystemTenantId);
	}

	/// <summary>
	/// Sistem rollerini oluşturur.
	/// </summary>
	private async Task SeedSystemRolesAsync(CancellationToken cancellationToken)
	{
		var exists = await _context.Roles
			.IgnoreQueryFilters()
			.AnyAsync(r => r.Id == SuperAdminRoleId, cancellationToken);

		if (exists)
			return;

		// Super Admin rolü
		var superAdminRole = Role.Create(
			SystemTenantId,
			"Super Admin",
			"SUPER_ADMIN",
			RoleLevel.System,  // 4. parametre (pozisyonel)
			"Sistem yöneticisi - Tüm yetkilere sahip",  // description
			isSystemRole: true,
			sessionTimeoutMinutes: 30);

		typeof(Role).GetProperty("Id")!
			.SetValue(superAdminRole, SuperAdminRoleId);

		// Tenant Admin rolü
		var tenantAdminRole = Role.Create(
			SystemTenantId,
			"Tenant Admin",
			"TENANT_ADMIN",
			RoleLevel.Tenant,  // 4. parametre (pozisyonel)
			"Tenant yöneticisi",  // description
			isSystemRole: true,
			sessionTimeoutMinutes: 60);

		typeof(Role).GetProperty("Id")!
			.SetValue(tenantAdminRole, TenantAdminRoleId);

		await _context.Roles.AddRangeAsync([superAdminRole, tenantAdminRole], cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);

		// Super Admin'e tüm izinleri ata
		var allPermissions = await _context.Permissions.ToListAsync(cancellationToken);
		var rolePermissions = allPermissions.Select(p =>
			RolePermission.Create(SystemTenantId, SuperAdminRoleId, p.Id)).ToList();

		await _context.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("System roles created: SuperAdmin, TenantAdmin");
	}

	/// <summary>
	/// Super Admin kullanıcısını oluşturur.
	/// </summary>
	private async Task SeedSuperAdminAsync(CancellationToken cancellationToken)
	{
		// Validasyon
		if (string.IsNullOrWhiteSpace(_superAdminSettings.Email))
		{
			_logger.LogWarning("SuperAdmin:Email is not configured. Skipping super admin creation.");
			return;
		}

		if (string.IsNullOrWhiteSpace(_superAdminSettings.Password))
		{
			_logger.LogWarning("SuperAdmin:Password is not configured. Skipping super admin creation.");
			return;
		}

		var exists = await _context.Users
			.IgnoreQueryFilters()
			.AnyAsync(u => u.Id == SuperAdminUserId, cancellationToken);

		if (exists)
		{
			_logger.LogInformation("Super admin user already exists");
			return;
		}

		// Şifreyi hashle
		var passwordHash = _passwordHasher.HashPassword(_superAdminSettings.Password);

		// Super Admin kullanıcısı oluştur
		var superAdmin = User.Create(
			SystemTenantId,
			_superAdminSettings.Username,
			_superAdminSettings.Email,
			passwordHash,
			_superAdminSettings.FirstName,
			_superAdminSettings.LastName);

		typeof(User).GetProperty("Id")!
			.SetValue(superAdmin, SuperAdminUserId);

		// Email'i doğrulanmış olarak işaretle
		superAdmin.ConfirmEmail();
		superAdmin.Activate();

		await _context.Users.AddAsync(superAdmin, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);

		// Super Admin rolünü ata
		var userRole = UserRole.Create(
			SystemTenantId,
			SuperAdminUserId,
			SuperAdminRoleId);

		await _context.UserRoles.AddAsync(userRole, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(
			"Super admin user created: {Email}. Please change the password on first login!",
			_superAdminSettings.Email);
	}
}