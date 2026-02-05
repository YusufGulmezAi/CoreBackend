using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CoreBackend.UnitTests.Domain.Entities;

/// <summary>
/// Tenant entity unit testleri.
/// </summary>
public class TenantTests
{
	[Fact]
	public void Create_WithValidData_ShouldCreateTenant()
	{
		// Act
		var tenant = Tenant.Create("Test Tenant", "test@email.com", "+905551234567", 10, 60);

		// Assert
		tenant.Should().NotBeNull();
		tenant.Name.Should().Be("Test Tenant");
		tenant.Email.Should().Be("test@email.com");
		tenant.Phone.Should().Be("+905551234567");
		tenant.MaxCompanyCount.Should().Be(10);
		tenant.SessionTimeoutMinutes.Should().Be(60);
		tenant.Status.Should().Be(TenantStatus.Active);
		tenant.Id.Should().NotBeEmpty();
	}

	[Fact]
	public void Create_WithMinimalData_ShouldCreateTenantWithDefaults()
	{
		// Act
		var tenant = Tenant.Create("Minimal Tenant", "minimal@email.com");

		// Assert
		tenant.Should().NotBeNull();
		tenant.MaxCompanyCount.Should().Be(5);
		tenant.SessionTimeoutMinutes.Should().BeNull();
	}

	[Fact]
	public void Activate_ShouldSetStatusToActive()
	{
		// Arrange
		var tenant = Tenant.Create("Test", "test@email.com");
		tenant.Deactivate();

		// Act
		tenant.Activate();

		// Assert
		tenant.Status.Should().Be(TenantStatus.Active);
	}

	[Fact]
	public void Deactivate_ShouldSetStatusToInactive()
	{
		// Arrange
		var tenant = Tenant.Create("Test", "test@email.com");

		// Act
		tenant.Deactivate();

		// Assert
		tenant.Status.Should().Be(TenantStatus.Inactive);
	}

	[Fact]
	public void Suspend_ShouldSetStatusToSuspended()
	{
		// Arrange
		var tenant = Tenant.Create("Test", "test@email.com");

		// Act
		tenant.Suspend();

		// Assert
		tenant.Status.Should().Be(TenantStatus.Suspended);
	}

	[Fact]
	public void Update_ShouldUpdateTenantInfo()
	{
		// Arrange
		var tenant = Tenant.Create("Original", "original@email.com", "+905550000000");

		// Act
		tenant.Update("Updated", "updated@email.com", "+905551111111");

		// Assert
		tenant.Name.Should().Be("Updated");
		tenant.Email.Should().Be("updated@email.com");
		tenant.Phone.Should().Be("+905551111111");
	}

	[Fact]
	public void RenewSubscription_ShouldUpdateEndDateAndActivate()
	{
		// Arrange
		var tenant = Tenant.Create("Test", "test@email.com");
		tenant.Suspend();
		var newEndDate = DateTime.UtcNow.AddYears(1);

		// Act
		tenant.RenewSubscription(newEndDate, 20);

		// Assert
		tenant.SubscriptionEndDate.Should().Be(newEndDate);
		tenant.MaxCompanyCount.Should().Be(20);
		tenant.Status.Should().Be(TenantStatus.Active);
	}

	[Fact]
	public void UpdateSessionTimeout_ShouldUpdateTimeout()
	{
		// Arrange
		var tenant = Tenant.Create("Test", "test@email.com");

		// Act
		tenant.UpdateSessionTimeout(120);

		// Assert
		tenant.SessionTimeoutMinutes.Should().Be(120);
	}
}