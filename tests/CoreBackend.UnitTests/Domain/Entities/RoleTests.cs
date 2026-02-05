using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;
using CoreBackend.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace CoreBackend.UnitTests.Domain.Entities;

/// <summary>
/// Role entity testleri.
/// 
/// İŞ KURALLARI:
/// -------------
/// 1. Her rol bir tenant'a bağlı olmalı
/// 2. Rol kodu benzersiz olmalı
/// 3. System rolleri güncellenemez ve deaktif edilemez
/// 4. Rol seviyeleri: System > Tenant > Company > User
/// 5. Varsayılan olarak roller aktif oluşturulur
/// 
/// TEST KATEGORİLERİ:
/// ------------------
/// - Create: Rol oluşturma senaryoları
/// - Update: Güncelleme ve kısıtlamalar
/// - Status: Activate/Deactivate işlemleri
/// - SystemRole: Sistem rolü özel davranışları
/// - SessionTimeout: Oturum zaman aşımı
/// </summary>
/// <summary>
/// Role entity unit testleri.
/// </summary>
public class RoleTests
{
	private readonly Guid _tenantId = Guid.NewGuid();

	[Fact]
	public void Create_WithValidData_ShouldCreateRole()
	{
		// Act
		var role = Role.Create(_tenantId, "Admin", "ADMIN", RoleLevel.Tenant, "Administrator role");

		// Assert
		role.Should().NotBeNull();
		role.TenantId.Should().Be(_tenantId);
		role.Name.Should().Be("Admin");
		role.Code.Should().Be("ADMIN");
		role.Level.Should().Be(RoleLevel.Tenant);
		role.Description.Should().Be("Administrator role");
		role.IsActive.Should().BeTrue();
		role.IsSystemRole.Should().BeFalse();
	}

	[Fact]
	public void Create_AsSystemRole_ShouldSetIsSystemRoleTrue()
	{
		// Act
		var role = Role.Create(_tenantId, "SuperAdmin", "SUPERADMIN", RoleLevel.System, "System admin", true);

		// Assert
		role.IsSystemRole.Should().BeTrue();
	}

	[Fact]
	public void Update_WhenNotSystemRole_ShouldUpdateNameAndDescription()
	{
		// Arrange
		var role = Role.Create(_tenantId, "Original", "ORIG", RoleLevel.Tenant);

		// Act
		role.Update("Updated", "Updated description");

		// Assert
		role.Name.Should().Be("Updated");
		role.Description.Should().Be("Updated description");
	}

	[Fact]
	public void Update_WhenSystemRole_ShouldNotUpdate()
	{
		// Arrange
		var role = Role.Create(_tenantId, "SystemRole", "SYSTEM", RoleLevel.System, "Original", true);

		// Act
		role.Update("Updated", "Updated description");

		// Assert
		role.Name.Should().Be("SystemRole");
		role.Description.Should().Be("Original");
	}

	[Fact]
	public void Deactivate_WhenNotSystemRole_ShouldDeactivate()
	{
		// Arrange
		var role = Role.Create(_tenantId, "Regular", "REG", RoleLevel.Tenant);

		// Act
		role.Deactivate();

		// Assert
		role.IsActive.Should().BeFalse();
	}

	[Fact]
	public void Deactivate_WhenSystemRole_ShouldNotDeactivate()
	{
		// Arrange
		var role = Role.Create(_tenantId, "System", "SYS", RoleLevel.System, null, true);

		// Act
		role.Deactivate();

		// Assert
		role.IsActive.Should().BeTrue();
	}

	[Fact]
	public void Activate_ShouldActivateRole()
	{
		// Arrange
		var role = Role.Create(_tenantId, "Test", "TEST", RoleLevel.Tenant);
		role.Deactivate();

		// Act
		role.Activate();

		// Assert
		role.IsActive.Should().BeTrue();
	}
}