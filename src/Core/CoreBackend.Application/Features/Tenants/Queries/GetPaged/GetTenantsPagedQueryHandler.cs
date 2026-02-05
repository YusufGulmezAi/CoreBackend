using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Common;
using CoreBackend.Contracts.Tenants.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Application.Features.Tenants.Queries.GetPaged;

public class GetTenantsPagedQueryHandler : IRequestHandler<GetTenantsPagedQuery, Result<PaginatedList<TenantListResponse>>>
{
	private readonly IUnitOfWork _unitOfWork;

	public GetTenantsPagedQueryHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<PaginatedList<TenantListResponse>>> Handle(
		GetTenantsPagedQuery request,
		CancellationToken cancellationToken)
	{
		var query = _unitOfWork.Tenants
			.AsNoTracking()
			.Include(t => t.Companies)
			.Include(t => t.Users)
			.AsQueryable();

		// Search
		if (!string.IsNullOrEmpty(request.SearchText))
		{
			var searchLower = request.SearchText.ToLower();
			query = query.Where(t =>
				t.Name.ToLower().Contains(searchLower) ||
				t.Email.ToLower().Contains(searchLower) ||
				(t.Subdomain != null && t.Subdomain.ToLower().Contains(searchLower)));
		}

		// Status filter
		if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<TenantStatus>(request.Status, true, out var status))
		{
			query = query.Where(t => t.Status == status);
		}

		// Sort
		query = request.SortBy?.ToLower() switch
		{
			"name" => request.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
			"email" => request.SortDescending ? query.OrderByDescending(t => t.Email) : query.OrderBy(t => t.Email),
			"createdat" => request.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
			"status" => request.SortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
			_ => query.OrderByDescending(t => t.CreatedAt)
		};

		var totalCount = await query.CountAsync(cancellationToken);

		var items = await query
			.Skip((request.PageNumber - 1) * request.PageSize)
			.Take(request.PageSize)
			.ToListAsync(cancellationToken);

		var response = items.Select(MapToListResponse).ToList();

		return Result.Success(new PaginatedList<TenantListResponse>
		{
			Items = response,
			TotalCount = totalCount,
			PageNumber = request.PageNumber,
			PageSize = request.PageSize
		});
	}

	private static TenantListResponse MapToListResponse(Tenant tenant) => new TenantListResponse
	{
		Id = tenant.Id,
		Name = tenant.Name,
		Email = tenant.Email,
		Phone = tenant.Phone,
		Subdomain = tenant.Subdomain,
		ContactEmail = tenant.ContactEmail,
		ContactPhone = tenant.ContactPhone,
		Status = tenant.Status,
		MaxCompanyCount = tenant.MaxCompanyCount,
		SessionTimeoutMinutes = tenant.SessionTimeoutMinutes,
		SubscriptionStartDate = tenant.SubscriptionStartDate,
		SubscriptionEndDate = tenant.SubscriptionEndDate,
		CreatedAt = tenant.CreatedAt,
		CompanyCount = tenant.Companies?.Count ?? 0,
		UserCount = tenant.Users?.Count ?? 0
	};
}