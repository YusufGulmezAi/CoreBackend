using FluentValidation;
using CoreBackend.Domain.Constants;

namespace CoreBackend.Application.Features.Users.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
	public CreateUserCommandValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is required.")
			.MinimumLength(EntityConstants.User.UsernameMinLength)
			.MaximumLength(EntityConstants.User.UsernameMaxLength);

		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress();

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required.")
			.MinimumLength(8)
			.Matches("[A-Z]").WithMessage("Password must contain uppercase.")
			.Matches("[a-z]").WithMessage("Password must contain lowercase.")
			.Matches("[0-9]").WithMessage("Password must contain digit.")
			.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain special character.");

		RuleFor(x => x.FirstName)
			.NotEmpty().WithMessage("First name is required.")
			.MaximumLength(EntityConstants.User.FirstNameMaxLength);

		RuleFor(x => x.LastName)
			.NotEmpty().WithMessage("Last name is required.")
			.MaximumLength(EntityConstants.User.LastNameMaxLength);
	}
}