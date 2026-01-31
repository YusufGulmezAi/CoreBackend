using Microsoft.EntityFrameworkCore;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Common.Interfaces;
using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Infrastructure.Persistence.Context;

/// <summary>
/// Ana veritabanı context'i.
/// Multi-tenancy ve soft delete destekler.
/// </summary>
public class ApplicationDbContext : DbContext
{
	private readonly ITenantService _tenantService;
	private readonly ICurrentUserService _currentUserService;

	public ApplicationDbContext(
		DbContextOptions<ApplicationDbContext> options,
		ITenantService tenantService,
		ICurrentUserService currentUserService) : base(options)
	{
		_tenantService = tenantService;
		_currentUserService = currentUserService;
	}

	// Entity DbSet'leri
	public DbSet<Tenant> Tenants => Set<Tenant>();
	public DbSet<Company> Companies => Set<Company>();
	public DbSet<User> Users => Set<User>();
	public DbSet<Role> Roles => Set<Role>();
	public DbSet<Permission> Permissions => Set<Permission>();
	public DbSet<UserRole> UserRoles => Set<UserRole>();
	public DbSet<UserCompanyRole> UserCompanyRoles => Set<UserCompanyRole>();
	public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
	public DbSet<UserSession> UserSessions => Set<UserSession>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Tüm configuration'ları otomatik uygula
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

		// Global Query Filters
		ApplyGlobalFilters(modelBuilder);
	}

	/// <summary>
	/// Global query filter'ları uygular.
	/// Soft delete ve multi-tenancy için.
	/// </summary>
	private void ApplyGlobalFilters(ModelBuilder modelBuilder)
	{
		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			// Soft Delete Filter
			if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
			{
				var method = typeof(ApplicationDbContext)
					.GetMethod(nameof(ApplySoftDeleteFilter),
						System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
					.MakeGenericMethod(entityType.ClrType);

				method.Invoke(null, new object[] { modelBuilder });
			}

			// Tenant Filter
			if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
			{
				var method = typeof(ApplicationDbContext)
					.GetMethod(nameof(ApplyTenantFilter),
						System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
					.MakeGenericMethod(entityType.ClrType);

				method.Invoke(this, new object[] { modelBuilder });
			}
		}
	}

	private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
		where TEntity : class, ISoftDeletable
	{
		modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
	}

	private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
		where TEntity : class, ITenantEntity
	{
		modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
			!_tenantService.HasTenant || e.TenantId == _tenantService.TenantId);
	}

	/// <summary>
	/// SaveChanges öncesi audit alanlarını otomatik doldurur.
	/// </summary>
	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		foreach (var entry in ChangeTracker.Entries<IAuditable>())
		{
			switch (entry.State)
			{
				case EntityState.Added:
					entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = DateTime.UtcNow;
					entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue = _currentUserService.UserId;
					break;

				case EntityState.Modified:
					entry.Property(nameof(IAuditable.ModifiedAt)).CurrentValue = DateTime.UtcNow;
					entry.Property(nameof(IAuditable.ModifiedBy)).CurrentValue = _currentUserService.UserId;
					break;
			}
		}

		// Tenant Id otomatik ata
		foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
		{
			if (entry.State == EntityState.Added && _tenantService.HasTenant)
			{
				var tenantIdProperty = entry.Property(nameof(ITenantEntity.TenantId));
				if ((Guid)tenantIdProperty.CurrentValue! == Guid.Empty)
				{
					tenantIdProperty.CurrentValue = _tenantService.TenantId;
				}
			}
		}

		return await base.SaveChangesAsync(cancellationToken);
	}
}