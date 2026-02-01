using FluentValidation;
using CoreBackend.Domain.Constants;

namespace CoreBackend.Application.Features.Companies.Commands.Create;

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
	public CreateCompanyCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Company name is required.")
			.MaximumLength(EntityConstants.Company.NameMaxLength);

		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Company code is required.")
			.MaximumLength(EntityConstants.Company.CodeMaxLength)
			.Matches("^[A-Z0-9_]+$").WithMessage("Code must contain only uppercase letters, numbers and underscore.");

		RuleFor(x => x.TaxNumber)
			.MaximumLength(EntityConstants.Company.TaxNumberMaxLength)
			.When(x => !string.IsNullOrEmpty(x.TaxNumber));

		RuleFor(x => x.Email)
			.EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
	}
}