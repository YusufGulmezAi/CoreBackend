using FluentValidation;

namespace CoreBackend.Application.Features.Tenants.Commands.Create;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
	public CreateTenantCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Tenant name is required.")
			.MaximumLength(200).WithMessage("Tenant name cannot exceed 200 characters.");

		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress().WithMessage("Invalid email format.")
			.MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

		RuleFor(x => x.Phone)
			.MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.");

		RuleFor(x => x.Subdomain)
			.MaximumLength(100).WithMessage("Subdomain cannot exceed 100 characters.")
			.Matches(@"^[a-z0-9-]+$").When(x => !string.IsNullOrEmpty(x.Subdomain))
			.WithMessage("Subdomain can only contain lowercase letters, numbers, and hyphens.");

		RuleFor(x => x.MaxCompanyCount)
			.GreaterThan(0).WithMessage("Max company count must be greater than 0.");
	}
}