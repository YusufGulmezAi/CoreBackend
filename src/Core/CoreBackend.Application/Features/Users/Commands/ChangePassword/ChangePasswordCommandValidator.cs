using FluentValidation;

namespace CoreBackend.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
	public ChangePasswordCommandValidator()
	{
		RuleFor(x => x.CurrentPassword)
			.NotEmpty().WithMessage("Current password is required.");

		RuleFor(x => x.NewPassword)
			.NotEmpty().WithMessage("New password is required.")
			.MinimumLength(8)
			.Matches("[A-Z]").WithMessage("Password must contain uppercase.")
			.Matches("[a-z]").WithMessage("Password must contain lowercase.")
			.Matches("[0-9]").WithMessage("Password must contain digit.")
			.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain special character.");

		RuleFor(x => x.ConfirmNewPassword)
			.Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
	}
}