using FluentValidation;
using CoreBackend.Domain.Constants;

namespace CoreBackend.Application.Features.Auth.Commands.Login;

/// <summary>
/// Login command validator.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
	public LoginCommandValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress().WithMessage("Invalid email format.")
			.MaximumLength(EntityConstants.User.EmailMaxLength)
				.WithMessage($"Email cannot exceed {EntityConstants.User.EmailMaxLength} characters.");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required.");
	}
}