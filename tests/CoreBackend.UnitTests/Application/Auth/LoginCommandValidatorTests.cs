using CoreBackend.Application.Features.Auth.Commands.Login;
using FluentValidation.TestHelper;
using Xunit;

namespace CoreBackend.UnitTests.Application.Auth;

/// <summary>
/// Login command validator testleri.
/// 
/// MEVCUT VALIDATOR KURALLARI:
/// - Email: NotEmpty, EmailAddress format, MaxLength
/// - Password: NotEmpty (minimum uzunluk YOK)
/// </summary>
public class LoginCommandValidatorTests
{
	private readonly LoginCommandValidator _validator;

	public LoginCommandValidatorTests()
	{
		_validator = new LoginCommandValidator();
	}

	#region Valid Credentials

	[Fact]
	public void Validate_WithValidCredentials_ShouldNotHaveErrors()
	{
		// Arrange
		var command = new LoginCommand(
			Email: "valid@email.com",
			Password: "ValidPassword123!",
			RememberMe: false);

		// Act
		var result = _validator.TestValidate(command);

		// Assert
		result.ShouldNotHaveAnyValidationErrors();
	}

	#endregion

	#region Email Validation

	[Fact]
	public void Validate_WithEmptyEmail_ShouldHaveError()
	{
		var command = new LoginCommand("", "password", false);
		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.Email);
	}

	[Fact]
	public void Validate_WithInvalidEmail_ShouldHaveError()
	{
		var command = new LoginCommand("invalid-email", "password", false);
		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.Email);
	}

	[Theory]
	[InlineData("plaintext")]
	[InlineData("@nodomain.com")]
	[InlineData("user@")]
	public void Validate_WithVariousInvalidEmails_ShouldHaveError(string invalidEmail)
	{
		var command = new LoginCommand(invalidEmail, "password", false);
		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.Email);
	}

	#endregion

	#region Password Validation

	[Fact]
	public void Validate_WithEmptyPassword_ShouldHaveError()
	{
		var command = new LoginCommand("valid@email.com", "", false);
		var result = _validator.TestValidate(command);
		result.ShouldHaveValidationErrorFor(x => x.Password);
	}

	/// <summary>
	/// Validator'da minimum şifre uzunluğu kontrolü yok.
	/// Kısa şifreler de geçerli kabul edilir.
	/// </summary>
	[Theory]
	[InlineData("a")]
	[InlineData("ab")]
	[InlineData("abc")]
	public void Validate_WithShortPassword_ShouldNotHaveError(string shortPassword)
	{
		var command = new LoginCommand("valid@email.com", shortPassword, false);
		var result = _validator.TestValidate(command);
		result.ShouldNotHaveValidationErrorFor(x => x.Password);
	}

	#endregion
}