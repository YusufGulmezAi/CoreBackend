using FluentValidation;
using CoreBackend.Domain.Constants;

namespace CoreBackend.Application.Features.Auth.Commands.Register;

/// <summary>
/// Register command validator.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
	public RegisterCommandValidator()
	{
		RuleFor(x => x.TenantName)
			.NotEmpty().WithMessage("Tenant name is required.")
			.MinimumLength(EntityConstants.Tenant.NameMinLength)
				.WithMessage($"Tenant name must be at least {EntityConstants.Tenant.NameMinLength} characters.")
			.MaximumLength(EntityConstants.Tenant.NameMaxLength)
				.WithMessage($"Tenant name cannot exceed {EntityConstants.Tenant.NameMaxLength} characters.");

		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is required.")
			.MinimumLength(EntityConstants.User.UsernameMinLength)
				.WithMessage($"Username must be at least {EntityConstants.User.UsernameMinLength} characters.")
			.MaximumLength(EntityConstants.User.UsernameMaxLength)
				.WithMessage($"Username cannot exceed {EntityConstants.User.UsernameMaxLength} characters.");

		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress().WithMessage("Invalid email format.")
			.MaximumLength(EntityConstants.User.EmailMaxLength)
				.WithMessage($"Email cannot exceed {EntityConstants.User.EmailMaxLength} characters.");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required.")
			.MinimumLength(8).WithMessage("Password must be at least 8 characters.")
			.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
			.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
			.Matches("[0-9]").WithMessage("Password must contain at least one digit.")
			.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

		RuleFor(x => x.ConfirmPassword)
			.NotEmpty().WithMessage("Confirm password is required.")
			.Equal(x => x.Password).WithMessage("Passwords do not match.");

		RuleFor(x => x.FirstName)
			.NotEmpty().WithMessage("First name is required.")
			.MinimumLength(EntityConstants.User.FirstNameMinLength)
				.WithMessage($"First name must be at least {EntityConstants.User.FirstNameMinLength} characters.")
			.MaximumLength(EntityConstants.User.FirstNameMaxLength)
				.WithMessage($"First name cannot exceed {EntityConstants.User.FirstNameMaxLength} characters.");

		RuleFor(x => x.LastName)
			.NotEmpty().WithMessage("Last name is required.")
			.MinimumLength(EntityConstants.User.LastNameMinLength)
				.WithMessage($"Last name must be at least {EntityConstants.User.LastNameMinLength} characters.")
			.MaximumLength(EntityConstants.User.LastNameMaxLength)
				.WithMessage($"Last name cannot exceed {EntityConstants.User.LastNameMaxLength} characters.");

		RuleFor(x => x.Phone)
			.MaximumLength(EntityConstants.User.PhoneMaxLength)
				.WithMessage($"Phone cannot exceed {EntityConstants.User.PhoneMaxLength} characters.")
			.When(x => !string.IsNullOrEmpty(x.Phone));
	}
}