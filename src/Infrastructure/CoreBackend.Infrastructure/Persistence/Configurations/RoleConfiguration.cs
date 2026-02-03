using CoreBackend.Domain.Constants;
using CoreBackend.Domain.Entities;
using CoreBackend.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : TenantEntityConfiguration<Role>
{
	public void Configure(EntityTypeBuilder<Role> builder)
	{
		builder.ToTable("Roles");

		builder.HasKey(x => x.Id);

		// Soft Delete Query Filter
		builder.HasQueryFilter(x => !x.IsDeleted);

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(EntityConstants.Role.NameMaxLength);

		builder.Property(x => x.Code)
			.IsRequired()
			.HasMaxLength(EntityConstants.Role.CodeMaxLength);

		builder.Property(x => x.Description)
			.HasMaxLength(EntityConstants.Role.DescriptionMaxLength);

		// Filtered Indexes
		builder.HasIndex(x => x.TenantId)
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Roles_TenantId_Active");

		builder.HasIndex(x => new { x.TenantId, x.Code })
			.IsUnique()
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Roles_TenantId_Code_Unique_Active");

		builder.HasIndex(x => new { x.TenantId, x.Level })
			.HasFilter("\"IsDeleted\" = false AND \"IsActive\" = true")
			.HasDatabaseName("IX_Roles_TenantId_Level_Active");

		// NOT: Tenant relationship TenantConfiguration'da tanımlı
	}
}