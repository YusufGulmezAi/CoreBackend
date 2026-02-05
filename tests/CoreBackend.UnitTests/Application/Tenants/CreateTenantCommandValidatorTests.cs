using CoreBackend.Application.Features.Tenants.Commands.Create;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace CoreBackend.UnitTests.Application.Tenants;

/// <summary>
/// CreateTenantCommand validator testleri.
/// 
/// VALIDATOR TESTLERİ NEDEN ÖNEMLİ?
/// --------------------------------
/// 1. Geçersiz verinin sisteme girmesini önler
/// 2. API'ye gelen istekler handler'a ulaşmadan filtrelenir
/// 3. Kullanıcıya anlamlı hata mesajları döner
/// 4. Güvenlik: SQL injection, XSS gibi saldırıları engeller
/// 
/// TEST YAKLAŞIMI:
/// ---------------
/// - Pozitif test: Geçerli veri -> hata yok
/// - Negatif test: Geçersiz veri -> hata var
/// - Sınır değer testi: Max/Min uzunluklar
/// 
/// FLUENT VALIDATION TEST HELPER:
/// ------------------------------
/// TestValidate() - Validator'ı çalıştırır
/// ShouldNotHaveAnyValidationErrors() - Hata olmamalı
/// ShouldHaveValidationErrorFor(x => x.Property) - Bu alanda hata olmalı
/// </summary>
public class CreateTenantCommandValidatorTests
{
	private readonly CreateTenantCommandValidator _validator;

	public CreateTenantCommandValidatorTests()
	{
		_validator = new CreateTenantCommandValidator();
	}

	#region Valid Command Tests

	[Fact]
	public void Validate_WithValidCommand_ShouldNotHaveErrors()
	{
		// Arrange
		var command = new CreateTenantCommand(
			Name: "Valid Tenant Name",
			Email: "valid@email.com",
			Phone: "+905551234567",
			Subdomain: "valid-subdomain",
			MaxCompanyCount: 10,
			SessionTimeoutMinutes: 60);

		// Act
		var result = _validator.TestValidate(command);

		// Assert
		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion

	#region Name Validation Tests

	[Fact]
	public void Validate_WithEmptyName_ShouldHaveError()
	{
		var command = new CreateTenantCommand(Name: "", Email: "valid@email.com");
		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.Name);
	}

	[Fact]
	public void Validate_WithTooLongName_ShouldHaveError()
	{
		var command = new CreateTenantCommand(
			Name: new string('A', 201),
			Email: "valid@email.com");
		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.Name);
	}

	#endregion

	#region Email Validation Tests

	[Fact]
	public void Validate_WithInvalidEmail_ShouldHaveError()
	{
		var command = new CreateTenantCommand(Name: "Valid Name", Email: "invalid-email");
		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.Email);
	}

	[Fact]
	public void Validate_WithEmptyEmail_ShouldHaveError()
	{
		var command = new CreateTenantCommand(Name: "Valid Name", Email: "");
		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.Email);
	}

	/// <summary>
	/// FluentValidation EmailAddress() kuralının reddettiği formatlar.
	/// NOT: Bazı "geçersiz görünen" formatlar FluentValidation tarafından kabul edilebilir.
	/// </summary>
	[Theory]
	[InlineData("plaintext")]           // @ yok
	[InlineData("@nodomain.com")]       // Kullanıcı adı yok
	[InlineData("user@")]               // Domain yok
	public void Validate_WithVariousInvalidEmails_ShouldHaveError(string invalidEmail)
	{
		var command = new CreateTenantCommand(Name: "Valid Name", Email: invalidEmail);
		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.Email);
	}

	#endregion

	#region Subdomain Validation Tests

	[Theory]
	[InlineData("valid-subdomain")]
	[InlineData("tenant123")]
	[InlineData("my-company-2024")]
	public void Validate_WithValidSubdomain_ShouldNotHaveError(string subdomain)
	{
		var command = new CreateTenantCommand(
			Name: "Valid Name",
			Email: "valid@email.com",
			Subdomain: subdomain);

		var result = _validator.TestValidate(command);
		result.ShouldNotHaveValidationErrorFor(x => x.Subdomain);
	}

	[Theory]
	[InlineData("Invalid Subdomain")]  // Boşluk var
	[InlineData("UPPERCASE")]          // Büyük harf
	[InlineData("special@chars")]      // Özel karakter
	public void Validate_WithInvalidSubdomain_ShouldHaveError(string subdomain)
	{
		var command = new CreateTenantCommand(
			Name: "Valid Name",
			Email: "valid@email.com",
			Subdomain: subdomain);

		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.Subdomain);
	}

	[Fact]
	public void Validate_WithNullSubdomain_ShouldNotHaveError()
	{
		var command = new CreateTenantCommand(
			Name: "Valid Name",
			Email: "valid@email.com",
			Subdomain: null);

		var result = _validator.TestValidate(command);
		result.ShouldNotHaveValidationErrorFor(x => x.Subdomain);
	}

	#endregion

	#region MaxCompanyCount Validation Tests

	[Fact]
	public void Validate_WithZeroMaxCompanyCount_ShouldHaveError()
	{
		var command = new CreateTenantCommand(
			Name: "Valid Name",
			Email: "valid@email.com",
			MaxCompanyCount: 0);

		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.MaxCompanyCount);
	}

	[Fact]
	public void Validate_WithNegativeMaxCompanyCount_ShouldHaveError()
	{
		var command = new CreateTenantCommand(
			Name: "Valid Name",
			Email: "valid@email.com",
			MaxCompanyCount: -5);

		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.MaxCompanyCount);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(5)]
	[InlineData(100)]
	public void Validate_WithPositiveMaxCompanyCount_ShouldNotHaveError(int count)
	{
		var command = new CreateTenantCommand(
			Name: "Valid Name",
			Email: "valid@email.com",
			MaxCompanyCount: count);

		var result = _validator.TestValidate(command);
		result.ShouldNotHaveValidationErrorFor(x => x.MaxCompanyCount);
	}

	#endregion
}