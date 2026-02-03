using CoreBackend.Domain.Entities;
using CoreBackend.Infrastructure.Persistence.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
	public void Configure(EntityTypeBuilder<UserSession> builder)
	{
		builder.ToTable("UserSessions");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.RefreshToken)
			.HasMaxLength(500);

		builder.Property(x => x.IpAddress)
			.HasMaxLength(45);

		builder.Property(x => x.UserAgent)
			.HasMaxLength(500);

		// Indexes
		builder.HasIndex(x => x.TenantId)
			.HasDatabaseName("IX_UserSessions_TenantId");

		builder.HasIndex(x => x.UserId)
			.HasDatabaseName("IX_UserSessions_UserId");

		// IsActive method olduğu için IsRevoked kullanılıyor
		builder.HasIndex(x => new { x.UserId, x.IsRevoked })
			.HasDatabaseName("IX_UserSessions_UserId_IsRevoked");

		// Relationships - Tenant hariç
		builder.HasOne(t=>t.Tenant)
			.WithMany(t=> t.UserSessions)
			.HasForeignKey(x => x.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

		// User navigation property entity'de tanımlı olmadığı için generic syntax kullanılıyor
		builder.HasOne<User>()
			.WithMany()
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}