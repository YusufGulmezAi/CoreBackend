using MediatR;
using CoreBackend.Contracts.Common;
using CoreBackend.Contracts.Tenants.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Tenants.Queries.GetPaged;

public record GetTenantsPagedQuery(
	int PageNumber = 1,
	int PageSize = 10,
	string? SearchText = null,
	string? Status = null,
	string? SortBy = null,
	bool SortDescending = false
) : IRequest<Result<PaginatedList<TenantListResponse>>>;