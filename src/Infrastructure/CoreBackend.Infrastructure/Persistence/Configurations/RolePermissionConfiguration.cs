using CoreBackend.Domain.Entities;
using CoreBackend.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : TenantEntityConfiguration<RolePermission>
{
	public void Configure(EntityTypeBuilder<RolePermission> builder)
	{
		builder.ToTable("RolePermissions");

		builder.HasKey(x => x.Id);

		// Soft Delete Query Filter
		builder.HasQueryFilter(x => !x.IsDeleted);

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

		// Relationships - Tenant hariç
		builder.HasOne<Tenant>()
			.WithMany()
			.HasForeignKey(x => x.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

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