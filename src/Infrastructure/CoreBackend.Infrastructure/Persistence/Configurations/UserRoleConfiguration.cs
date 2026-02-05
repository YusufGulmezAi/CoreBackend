using CoreBackend.Domain.Entities;
using CoreBackend.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : TenantEntityConfiguration<UserRole>
{
	public override void Configure(EntityTypeBuilder<UserRole> builder)
	{
		// Base configuration'ı çağır (soft delete filter)
		base.Configure(builder);

		builder.ToTable("UserRoles");

		builder.HasKey(x => x.Id);

		// Filtered Indexes
		builder.HasIndex(x => x.TenantId)
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_UserRoles_TenantId_Active");

		builder.HasIndex(x => new { x.TenantId, x.UserId, x.RoleId })
			.IsUnique()
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_UserRoles_TenantId_UserId_RoleId_Unique_Active");

		builder.HasIndex(x => new { x.TenantId, x.UserId })
			.HasFilter("\"IsDeleted\" = false AND \"IsActive\" = true")
			.HasDatabaseName("IX_UserRoles_TenantId_UserId_Active");

		// Relationships - Tenant ilişkisi KALDIRILDI (TenantId FK olarak yeterli)
		builder.HasOne(x => x.User)
			.WithMany(u => u.UserRoles)
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(x => x.Role)
			.WithMany(r => r.UserRoles)
			.HasForeignKey(x => x.RoleId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}