using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Constants;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
	public void Configure(EntityTypeBuilder<Tenant> builder)
	{
		builder.ToTable("Tenants");

		builder.HasKey(x => x.Id);

		// Soft Delete Query Filter
		builder.HasQueryFilter(x => !x.IsDeleted);

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(EntityConstants.Tenant.NameMaxLength);

		builder.Property(x => x.Subdomain)
			.HasMaxLength(EntityConstants.Tenant.SubdomainMaxLength);

		builder.Property(x => x.ContactEmail)
			.HasMaxLength(EntityConstants.Tenant.ContactEmailMaxLength);

		builder.Property(x => x.ContactPhone)
			.HasMaxLength(EntityConstants.Tenant.ContactPhoneMaxLength);

		// 2FA Policy
		builder.Property(x => x.TwoFactorPolicy)
			.HasConversion<int>();

		// AllowedTwoFactorMethods - Value Converter ve Comparer ile
		builder.Property(x => x.AllowedTwoFactorMethods)
			.HasConversion(
				v => string.Join(',', v.Select(m => (int)m)),
				v => string.IsNullOrEmpty(v)
					? new List<TwoFactorMethod>()
					: v.Split(',', StringSplitOptions.RemoveEmptyEntries)
						.Select(s => (TwoFactorMethod)int.Parse(s))
						.ToList())
			.Metadata.SetValueComparer(new ValueComparer<ICollection<TwoFactorMethod>>(
				(c1, c2) => c1!.SequenceEqual(c2!),
				c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
				c => c.ToList()));

		// Filtered Indexes
		builder.HasIndex(x => x.Subdomain)
			.IsUnique()
			.HasFilter("\"IsDeleted\" = false AND \"Subdomain\" IS NOT NULL")
			.HasDatabaseName("IX_Tenants_Subdomain_Unique_Active");

		builder.HasIndex(x => x.Status)
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Tenants_Status_Active");

		// Relationships - Tenant tarafından tanımlanıyor
		builder.HasMany(x => x.Users)
			.WithOne(u => u.Tenant)
			.HasForeignKey(u => u.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(x => x.Companies)
			.WithOne(c => c.Tenant)
			.HasForeignKey(c => c.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(x => x.Roles)
			.WithOne(r => r.Tenant)
			.HasForeignKey(r => r.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(x => x.UserSessions)
			.WithOne(us => us.Tenant)
			.HasForeignKey(us => us.TenantId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}