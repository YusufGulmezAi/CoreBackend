using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Companies.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Companies.Commands.Create;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Result<CompanyResponse>>
{
	private readonly IRepositoryExtended<Company, Guid> _companyRepository;
	private readonly ICurrentUserService _currentUserService;
	private readonly IUnitOfWork _unitOfWork;

	public CreateCompanyCommandHandler(
		IRepositoryExtended<Company, Guid> companyRepository,
		ICurrentUserService currentUserService,
		IUnitOfWork unitOfWork)
	{
		_companyRepository = companyRepository;
		_currentUserService = currentUserService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<CompanyResponse>> Handle(
		CreateCompanyCommand request,
		CancellationToken cancellationToken)
	{
		var tenantId = _currentUserService.TenantId;

		if (!tenantId.HasValue)
		{
			return Result.Failure<CompanyResponse>(
				Error.Create(ErrorCodes.Auth.Unauthorized, "Tenant not found."));
		}

		// Code benzersizlik kontrolü
		var existingCompany = await _companyRepository.FirstOrDefaultAsync(
			c => c.Code == request.Code,
			cancellationToken);

		if (existingCompany != null)
		{
			return Result.Failure<CompanyResponse>(
				Error.Create(ErrorCodes.Company.AlreadyExists, "Company code already exists."));
		}

		var company = Company.Create(
			tenantId.Value,
			request.Name,
			request.Code,
			request.TaxNumber,
			request.Address,
			request.Phone,
			request.Email);

		await _companyRepository.AddAsync(company, cancellationToken);
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