using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Companies.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Companies.Commands.Update;

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, Result<CompanyResponse>>
{
	private readonly IUnitOfWork _unitOfWork;

	public UpdateCompanyCommandHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<CompanyResponse>> Handle(
		UpdateCompanyCommand request,
		CancellationToken cancellationToken)
	{
		var company = await _unitOfWork.Companies
			.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

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