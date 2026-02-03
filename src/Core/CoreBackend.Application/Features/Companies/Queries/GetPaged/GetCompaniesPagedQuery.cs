using MediatR;
using CoreBackend.Contracts.Common;
using CoreBackend.Contracts.Companies.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Companies.Queries.GetPaged;

public record GetCompaniesPagedQuery(
	int PageNumber = 1,
	int PageSize = 10,
	string? SearchText = null,
	string? SortBy = null,
	bool SortDescending = false
) : IRequest<Result<PaginatedList<CompanyResponse>>>;