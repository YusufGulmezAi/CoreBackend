using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Tenants.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Tenants.Commands.Create;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantResponse>>
{
	private readonly IUnitOfWork _unitOfWork;

	public CreateTenantCommandHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<TenantResponse>> Handle(
		CreateTenantCommand request,
		CancellationToken cancellationToken)
	{
		// Email benzersizlik kontrolü
		var existingTenant = await _unitOfWork.Tenants
			.AsNoTracking()
			.FirstOrDefaultAsync(t => t.Email == request.Email, cancellationToken);

		if (existingTenant != null)
		{
			return Result.Failure<TenantResponse>(
				Error.Create(ErrorCodes.Tenant.AlreadyExists, "A tenant with this email already exists."));
		}

		// Subdomain benzersizlik kontrolü
		if (!string.IsNullOrEmpty(request.Subdomain))
		{
			var existingSubdomain = await _unitOfWork.Tenants
				.AsNoTracking()
				.FirstOrDefaultAsync(t => t.Subdomain == request.Subdomain, cancellationToken);

			if (existingSubdomain != null)
			{
				return Result.Failure<TenantResponse>(
					Error.Create(ErrorCodes.Tenant.AlreadyExists, "A tenant with this subdomain already exists."));
			}
		}

		var tenant = Tenant.Create(
			request.Name,
			request.Email,
			request.Phone,
			request.MaxCompanyCount,
			request.SessionTimeoutMinutes);

		if (!string.IsNullOrEmpty(request.Subdomain))
		{
			tenant.Subdomain = request.Subdomain;
		}

		await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

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
			UpdatedAt = tenant.UpdatedAt
		};
	}
}