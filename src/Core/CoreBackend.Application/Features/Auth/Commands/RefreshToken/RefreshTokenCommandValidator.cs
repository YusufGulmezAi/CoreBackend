using FluentValidation;

namespace CoreBackend.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Refresh token command validator.
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
	public RefreshTokenCommandValidator()
	{
		RuleFor(x => x.AccessToken)
			.NotEmpty().WithMessage("Access token is required.");

		RuleFor(x => x.RefreshToken)
			.NotEmpty().WithMessage("Refresh token is required.");
	}
}