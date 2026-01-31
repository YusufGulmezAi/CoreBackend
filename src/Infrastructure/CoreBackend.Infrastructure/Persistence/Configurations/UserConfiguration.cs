using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Constants;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

/// <summary>
/// User entity konfigürasyonu.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		// Tablo adı
		builder.ToTable("Users");

		// Primary Key
		builder.HasKey(u => u.Id);

		// Properties
		builder.Property(u => u.Username)
			.IsRequired()
			.HasMaxLength(EntityConstants.User.UsernameMaxLength);

		builder.Property(u => u.Email)
			.IsRequired()
			.HasMaxLength(EntityConstants.User.EmailMaxLength);

		builder.Property(u => u.PasswordHash)
			.IsRequired()
			.HasMaxLength(EntityConstants.User.PasswordHashMaxLength);

		builder.Property(u => u.FirstName)
			.IsRequired()
			.HasMaxLength(EntityConstants.User.FirstNameMaxLength);

		builder.Property(u => u.LastName)
			.IsRequired()
			.HasMaxLength(EntityConstants.User.LastNameMaxLength);

		builder.Property(u => u.Phone)
			.HasMaxLength(EntityConstants.User.PhoneMaxLength);

		builder.Property(u => u.Status)
			.IsRequired();

		builder.Property(u => u.RefreshToken)
			.HasMaxLength(EntityConstants.User.RefreshTokenMaxLength);

		builder.Property(u => u.Settings)
			.HasMaxLength(EntityConstants.Common.SettingsMaxLength);

		// Computed property - ignore
		builder.Ignore(u => u.FullName);

		// Indexes
		builder.HasIndex(u => new { u.TenantId, u.Username })
			.IsUnique();

		builder.HasIndex(u => new { u.TenantId, u.Email })
			.IsUnique();

		builder.HasIndex(u => u.TenantId);

		builder.HasIndex(u => u.Status);

		// Relationships
		builder.HasOne<Tenant>()
			.WithMany()
			.HasForeignKey(u => u.TenantId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}