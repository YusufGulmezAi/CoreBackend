using FluentValidation;
using CoreBackend.Domain.Constants;

namespace CoreBackend.Application.Features.Companies.Commands.Update;

public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
	public UpdateCompanyCommandValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty().WithMessage("Company Id is required.");

		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Company name is required.")
			.MaximumLength(EntityConstants.Company.NameMaxLength);

		RuleFor(x => x.Email)
			.EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
	}
}