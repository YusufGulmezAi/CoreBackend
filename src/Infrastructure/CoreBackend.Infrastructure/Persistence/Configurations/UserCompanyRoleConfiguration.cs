using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class UserCompanyRoleConfiguration : IEntityTypeConfiguration<UserCompanyRole>
{
	public void Configure(EntityTypeBuilder<UserCompanyRole> builder)
	{
		builder.ToTable("UserCompanyRoles");

		builder.HasKey(x => x.Id);

		// Soft Delete Query Filter
		builder.HasQueryFilter(x => !x.IsDeleted);

		// Filtered Indexes
		builder.HasIndex(x => x.TenantId)
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_UserCompanyRoles_TenantId_Active");

		builder.HasIndex(x => new { x.TenantId, x.UserId, x.CompanyId, x.RoleId })
			.IsUnique()
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_UserCompanyRoles_Unique_Active");

		builder.HasIndex(x => new { x.TenantId, x.UserId, x.CompanyId })
			.HasFilter("\"IsDeleted\" = false AND \"IsActive\" = true")
			.HasDatabaseName("IX_UserCompanyRoles_UserId_CompanyId_Active");

		// Relationships - Navigation property ile tanımla
		builder.HasOne(x => x.Tenant)
			.WithMany()
			.HasForeignKey(x => x.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(x => x.User)
			.WithMany(u => u.UserCompanyRoles)
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(x => x.Company)
			.WithMany(c => c.UserCompanyRoles)
			.HasForeignKey(x => x.CompanyId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(x => x.Role)
			.WithMany()
			.HasForeignKey(x => x.RoleId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}