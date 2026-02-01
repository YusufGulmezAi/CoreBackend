using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Models;
using CoreBackend.Contracts.Common;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using MediatR;
using CompanyResponseDto = CoreBackend.Contracts.Companies.Responses.CompanyResponse;
using PagedRequest = CoreBackend.Application.Common.Models.QueryOptions;
using PagedResponseDto = CoreBackend.Contracts.Common.PagedResponse<CoreBackend.Contracts.Companies.Responses.CompanyResponse>;

namespace CoreBackend.Application.Features.Companies.Queries.GetPaged;

public class GetCompaniesPagedQueryHandler
	: IRequestHandler<GetCompaniesPagedQuery, Result<PagedResponseDto>>
{
	private readonly IRepositoryExtended<Company, Guid> _companyRepository;

	public GetCompaniesPagedQueryHandler(IRepositoryExtended<Company, Guid> companyRepository)
	{
		_companyRepository = companyRepository;
	}

	public async Task<Result<PagedResponseDto>> Handle(
		GetCompaniesPagedQuery request,
		CancellationToken cancellationToken)
	{
		// Application'daki PagedRequest kullan
		var pagedRequest = new PagedRequest
		{
			PageNumber = request.PageNumber,
			PageSize = request.PageSize,
			SearchText = request.SearchText,
			SearchFields = new List<string> { "Name", "Code" }
		};

		if (!string.IsNullOrEmpty(request.SortBy))
		{
			pagedRequest.Query = new DynamicQuery
			{
				Sort = new List<SortDescriptor>
				{
					new()
					{
						Field = request.SortBy,
						Direction = request.SortDescending
							? SortDirection.Descending
							: SortDirection.Ascending
					}
				}
			};
		}

		var result = await _companyRepository.GetPagedAsync(pagedRequest, cancellationToken);

		// Contracts'taki PagedResponse'a dönüştür
		var response = new PagedResponseDto
		{
			Items = result.Items.Select(MapToResponse).ToList(),
			PageNumber = result.PageNumber,
			PageSize = result.PageSize,
			TotalCount = result.TotalCount
		};

		return Result.Success(response);
	}

	private static CompanyResponseDto MapToResponse(Company company)
	{
		return new CompanyResponseDto
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
			CreatedAt = company.CreatedAt
		};
	}
}