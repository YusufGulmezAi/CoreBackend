using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;
using CoreBackend.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace CoreBackend.UnitTests.Domain.Entities;

/// <summary>
/// Company entity testleri.
/// 
/// İŞ KURALLARI:
/// - Her şirket bir tenant'a bağlı olmalı
/// - Şirket kodu benzersiz olmalı
/// - Vergi numarası formatı doğru olmalı
/// </summary>
public class CompanyTests : TestBase
{
	[Fact]
	public void Create_WithValidData_ShouldCreateCompany()
	{
		// Arrange
		var tenantId = Guid.NewGuid();
		var name = "Test Company";
		var code = "TST001";
		var taxNumber = "1234567890";

		// Act
		var company = Company.Create(
			tenantId, name, code, taxNumber,
			"Test Address", "+905551234567", "test@company.com");

		// Assert
		company.Should().NotBeNull();
		company.Id.Should().NotBeEmpty();
		company.TenantId.Should().Be(tenantId);
		company.Name.Should().Be(name);
		company.Code.Should().Be(code);
		company.TaxNumber.Should().Be(taxNumber);
		company.Status.Should().Be(CompanyStatus.Active);
	}

	[Fact]
	public void Deactivate_ShouldSetStatusToInactive()
	{
		// Arrange
		var company = FakeDataGenerator.CreateCompany();

		// Act
		company.Deactivate();

		// Assert
		company.Status.Should().Be(CompanyStatus.Inactive);
	}

	[Fact]
	public void Activate_AfterDeactivation_ShouldSetStatusToActive()
	{
		// Arrange
		var company = FakeDataGenerator.CreateCompany();
		company.Deactivate();

		// Act
		company.Activate();

		// Assert
		company.Status.Should().Be(CompanyStatus.Active);
	}

	[Fact]
	public void Update_ShouldUpdateProperties()
	{
		// Arrange
		var company = FakeDataGenerator.CreateCompany();
		var newName = "Updated Company";
		var newTaxNumber = "9876543210";
		var newAddress = "New Address";
		var newPhone = "+905559999999";
		var newEmail = "new@company.com";

		// Act
		company.Update(newName, newTaxNumber, newAddress, newPhone, newEmail);

		// Assert
		company.Name.Should().Be(newName);
		company.TaxNumber.Should().Be(newTaxNumber);
		company.Address.Should().Be(newAddress);
		company.Phone.Should().Be(newPhone);
		company.Email.Should().Be(newEmail);
	}
}