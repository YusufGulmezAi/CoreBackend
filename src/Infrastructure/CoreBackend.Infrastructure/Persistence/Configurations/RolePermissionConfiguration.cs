using CoreBackend.Domain.Entities;
using CoreBackend.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : TenantEntityConfiguration<RolePermission>
{
	public override void Configure(EntityTypeBuilder<RolePermission> builder)
	{
		// Base configuration'ı çağır (soft delete filter)
		base.Configure(builder);

		builder.ToTable("RolePermissions");

		builder.HasKey(x => x.Id);

		// Filtered Indexes
		builder.HasIndex(x => x.TenantId)
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_RolePermissions_TenantId_Active");

		builder.HasIndex(x => new { x.TenantId, x.RoleId, x.PermissionId })
			.IsUnique()
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_RolePermissions_Unique_Active");

		builder.HasIndex(x => new { x.TenantId, x.RoleId })
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_RolePermissions_RoleId_Active");

		// Relationships - Tenant ilişkisi KALDIRILDI (TenantId FK olarak yeterli)
		builder.HasOne(x => x.Role)
			.WithMany(r => r.RolePermissions)
			.HasForeignKey(x => x.RoleId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(x => x.Permission)
			.WithMany(p => p.RolePermissions)
			.HasForeignKey(x => x.PermissionId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}