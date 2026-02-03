using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Constants;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
	public void Configure(EntityTypeBuilder<Permission> builder)
	{
		builder.ToTable("Permissions");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(EntityConstants.Permission.NameMaxLength);

		builder.Property(x => x.Code)
			.IsRequired()
			.HasMaxLength(EntityConstants.Permission.CodeMaxLength);

		builder.Property(x => x.Group)
			.IsRequired()
			.HasMaxLength(EntityConstants.Permission.GroupMaxLength);

		builder.Property(x => x.Description)
			.HasMaxLength(EntityConstants.Permission.DescriptionMaxLength);

		builder.Property(p => p.IsActive)
			.IsRequired();

		// Unique Index (Filtered)
		builder.HasIndex(x => x.Code)
			.IsUnique()
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Permissions_Code_Unique_Active");

		// Group index
		builder.HasIndex(x => x.Group)
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Permissions_Group_Active");

		// Soft Delete Query Filter
		builder.HasQueryFilter(x => !x.IsDeleted);
	}
}