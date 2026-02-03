using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Companies.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Companies.Queries.GetById;

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, Result<CompanyResponse>>
{
	private readonly IUnitOfWork _unitOfWork;

	public GetCompanyByIdQueryHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<CompanyResponse>> Handle(
		GetCompanyByIdQuery request,
		CancellationToken cancellationToken)
	{
		var company = await _unitOfWork.Companies
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

		if (company == null)
		{
			return Result.Failure<CompanyResponse>(
				Error.Create(ErrorCodes.Company.NotFound, "Company not found."));
		}

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