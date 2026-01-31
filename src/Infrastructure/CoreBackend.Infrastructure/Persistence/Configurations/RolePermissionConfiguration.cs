using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

/// <summary>
/// RolePermission entity konfigürasyonu.
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
	public void Configure(EntityTypeBuilder<RolePermission> builder)
	{
		// Tablo adı
		builder.ToTable("RolePermissions");

		// Primary Key
		builder.HasKey(rp => rp.Id);

		// Properties
		builder.Property(rp => rp.RoleId)
			.IsRequired();

		builder.Property(rp => rp.PermissionId)
			.IsRequired();

		// Indexes
		builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
			.IsUnique();

		builder.HasIndex(rp => rp.TenantId);

		builder.HasIndex(rp => rp.RoleId);

		builder.HasIndex(rp => rp.PermissionId);

		// Relationships
		builder.HasOne<Tenant>()
			.WithMany()
			.HasForeignKey(rp => rp.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne<Role>()
			.WithMany()
			.HasForeignKey(rp => rp.RoleId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne<Permission>()
			.WithMany()
			.HasForeignKey(rp => rp.PermissionId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}