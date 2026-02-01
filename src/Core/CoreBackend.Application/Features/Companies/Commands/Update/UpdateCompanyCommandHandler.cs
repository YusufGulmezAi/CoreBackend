using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Companies.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Companies.Commands.Update;

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, Result<CompanyResponse>>
{
	private readonly IRepositoryExtended<Company, Guid> _companyRepository;
	private readonly IUnitOfWork _unitOfWork;

	public UpdateCompanyCommandHandler(
		IRepositoryExtended<Company, Guid> companyRepository,
		IUnitOfWork unitOfWork)
	{
		_companyRepository = companyRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<CompanyResponse>> Handle(
		UpdateCompanyCommand request,
		CancellationToken cancellationToken)
	{
		var company = await _companyRepository.GetByIdAsync(request.Id, cancellationToken);

		if (company == null)
		{
			return Result.Failure<CompanyResponse>(
				Error.Create(ErrorCodes.Company.NotFound, "Company not found."));
		}

		company.Update(
			request.Name,
			request.TaxNumber,
			request.Address,
			request.Phone,
			request.Email);

		_companyRepository.Update(company);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success(MapToResponse(company));
	}

	private static CompanyResponse MapToResponse(Company company)
	{
		return new CompanyResponse
		{
			Id = company.Id,
			TenantId = company.TenantId,
			Name = company.Name,
			Code = company.Code,
			TaxNumber = company.TaxNumber,
			Address = company.Address,
			Phone = company.Phone,
			Email = company.Email,
			Status = company.Status.ToString(),
			CreatedAt = company.CreatedAt,
			UpdatedAt = company.UpdatedAt
		};
	}
}