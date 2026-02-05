using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Tenants.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Tenants.Commands.Update;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result<TenantResponse>>
{
	private readonly IUnitOfWork _unitOfWork;

	public UpdateTenantCommandHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<TenantResponse>> Handle(
		UpdateTenantCommand request,
		CancellationToken cancellationToken)
	{
		var tenant = await _unitOfWork.Tenants
			.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

		if (tenant == null)
		{
			return Result.Failure<TenantResponse>(
				Error.Create(ErrorCodes.Tenant.NotFound, "Tenant not found."));
		}

		// Email benzersizlik kontrolü
		var existingEmail = await _unitOfWork.Tenants
			.AsNoTracking()
			.FirstOrDefaultAsync(t => t.Email == request.Email && t.Id != request.Id, cancellationToken);

		if (existingEmail != null)
		{
			return Result.Failure<TenantResponse>(
				Error.Create(ErrorCodes.Tenant.AlreadyExists, "A tenant with this email already exists."));
		}

		// Subdomain benzersizlik kontrolü
		if (!string.IsNullOrEmpty(request.Subdomain))
		{
			var existingSubdomain = await _unitOfWork.Tenants
				.AsNoTracking()
				.FirstOrDefaultAsync(t => t.Subdomain == request.Subdomain && t.Id != request.Id, cancellationToken);

			if (existingSubdomain != null)
			{
				return Result.Failure<TenantResponse>(
					Error.Create(ErrorCodes.Tenant.AlreadyExists, "A tenant with this subdomain already exists."));
			}
		}

		tenant.Update(request.Name, request.Email, request.Phone);

		if (!string.IsNullOrEmpty(request.Subdomain))
			tenant.Subdomain = request.Subdomain;

		if (!string.IsNullOrEmpty(request.ContactEmail))
			tenant.ContactEmail = request.ContactEmail;

		if (!string.IsNullOrEmpty(request.ContactPhone))
			tenant.ContactPhone = request.ContactPhone;

		if (request.SessionTimeoutMinutes.HasValue)
			tenant.UpdateSessionTimeout(request.SessionTimeoutMinutes);

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