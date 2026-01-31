using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Constants;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

/// <summary>
/// UserSession entity konfigürasyonu.
/// </summary>
public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
	public void Configure(EntityTypeBuilder<UserSession> builder)
	{
		// Tablo adı
		builder.ToTable("UserSessions");

		// Primary Key
		builder.HasKey(us => us.Id);

		// Properties
		builder.Property(us => us.UserId)
			.IsRequired();

		builder.Property(us => us.SessionId)
			.IsRequired()
			.HasMaxLength(EntityConstants.UserSession.SessionIdMaxLength);

		builder.Property(us => us.RefreshToken)
			.HasMaxLength(EntityConstants.UserSession.RefreshTokenMaxLength);

		builder.Property(us => us.IpAddress)
			.IsRequired()
			.HasMaxLength(EntityConstants.UserSession.IpAddressMaxLength);

		builder.Property(us => us.UserAgent)
			.IsRequired()
			.HasMaxLength(EntityConstants.UserSession.UserAgentMaxLength);

		builder.Property(us => us.GeoLocation)
			.HasMaxLength(EntityConstants.UserSession.GeoLocationMaxLength);

		builder.Property(us => us.BrowserName)
			.HasMaxLength(EntityConstants.UserSession.BrowserNameMaxLength);

		builder.Property(us => us.OperatingSystem)
			.HasMaxLength(EntityConstants.UserSession.OperatingSystemMaxLength);

		builder.Property(us => us.DeviceType)
			.HasMaxLength(EntityConstants.UserSession.DeviceTypeMaxLength);

		builder.Property(us => us.StartedAt)
			.IsRequired();

		builder.Property(us => us.ExpiresAt)
			.IsRequired();

		builder.Property(us => us.LastActivityAt)
			.IsRequired();

		builder.Property(us => us.IsRevoked)
			.IsRequired();

		builder.Property(us => us.RevokedReason)
			.HasMaxLength(EntityConstants.UserSession.RevokedReasonMaxLength);

		builder.Property(us => us.AllowIpChange)
			.IsRequired();

		builder.Property(us => us.AllowUserAgentChange)
			.IsRequired();

		// Indexes
		builder.HasIndex(us => us.SessionId)
			.IsUnique();

		builder.HasIndex(us => us.TenantId);

		builder.HasIndex(us => us.UserId);

		builder.HasIndex(us => us.IsRevoked);

		builder.HasIndex(us => us.ExpiresAt);

		// Relationships
		builder.HasOne<Tenant>()
			.WithMany()
			.HasForeignKey(us => us.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne<User>()
			.WithMany()
			.HasForeignKey(us => us.UserId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}