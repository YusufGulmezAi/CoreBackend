using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Tenants.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Tenants.Queries.GetById;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantResponse>>
{
	private readonly IUnitOfWork _unitOfWork;

	public GetTenantByIdQueryHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<TenantResponse>> Handle(
		GetTenantByIdQuery request,
		CancellationToken cancellationToken)
	{
		var tenant = await _unitOfWork.Tenants
			.AsNoTracking()
			.Include(t => t.Companies)
			.Include(t => t.Users)
			.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

		if (tenant == null)
		{
			return Result.Failure<TenantResponse>(
				Error.Create(ErrorCodes.Tenant.NotFound, "Tenant not found."));
		}

		return Result.Success(MapToResponse(tenant));
	}

	private static TenantResponse MapToResponse(Tenant tenant)
	{
		return new TenantResponse
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
			UpdatedAt = tenant.UpdatedAt,
			CompanyCount = tenant.Companies?.Count ?? 0,
			UserCount = tenant.Users?.Count ?? 0
		};
	}
}