using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

/// <summary>
/// UserCompanyRole entity konfigürasyonu.
/// </summary>
public class UserCompanyRoleConfiguration : IEntityTypeConfiguration<UserCompanyRole>
{
	public void Configure(EntityTypeBuilder<UserCompanyRole> builder)
	{
		// Tablo adı
		builder.ToTable("UserCompanyRoles");

		// Primary Key
		builder.HasKey(ucr => ucr.Id);

		// Properties
		builder.Property(ucr => ucr.UserId)
			.IsRequired();

		builder.Property(ucr => ucr.RoleId)
			.IsRequired();

		builder.Property(ucr => ucr.AssignedAt)
			.IsRequired();

		builder.Property(ucr => ucr.IsActive)
			.IsRequired();

		// Indexes
		builder.HasIndex(ucr => new { ucr.UserId, ucr.CompanyId, ucr.RoleId })
			.IsUnique();

		builder.HasIndex(ucr => ucr.TenantId);

		builder.HasIndex(ucr => ucr.CompanyId);

		builder.HasIndex(ucr => ucr.UserId);

		builder.HasIndex(ucr => ucr.RoleId);

		// Relationships
		builder.HasOne<Tenant>()
			.WithMany()
			.HasForeignKey(ucr => ucr.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne<Company>()
			.WithMany()
			.HasForeignKey(ucr => ucr.CompanyId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne<User>()
			.WithMany()
			.HasForeignKey(ucr => ucr.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne<Role>()
			.WithMany()
			.HasForeignKey(ucr => ucr.RoleId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}