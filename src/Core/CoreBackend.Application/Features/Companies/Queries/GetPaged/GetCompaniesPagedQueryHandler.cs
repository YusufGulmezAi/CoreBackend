using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Models;
using CoreBackend.Contracts.Common;
using CoreBackend.Contracts.Companies.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Application.Features.Companies.Queries.GetPaged;

public class GetCompaniesPagedQueryHandler
	: IRequestHandler<GetCompaniesPagedQuery, Result<PagedResponse<CompanyResponse>>>
{
	private readonly IRepositoryExtended<Company, Guid> _companyRepository;

	public GetCompaniesPagedQueryHandler(IRepositoryExtended<Company, Guid> companyRepository)
	{
		_companyRepository = companyRepository;
	}

	public async Task<Result<PagedResponse<CompanyResponse>>> Handle(
		GetCompaniesPagedQuery request,
		CancellationToken cancellationToken)
	{
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
					new() { Field = request.SortBy, Direction = request.SortDescending ? SortDirection.Descending : SortDirection.Ascending }
				}
			};
		}

		var result = await _companyRepository.GetPagedAsync(pagedRequest, cancellationToken);

		var response = new PagedResponse<CompanyResponse>
		{
			Items = result.Items.Select(MapToResponse).ToList(),
			PageNumber = result.PageNumber,
			PageSize = result.PageSize,
			TotalCount = result.TotalCount
		};

		return Result.Success(response);
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