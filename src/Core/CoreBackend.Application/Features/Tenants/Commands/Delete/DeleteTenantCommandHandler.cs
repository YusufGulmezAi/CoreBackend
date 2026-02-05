using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Tenants.Commands.Delete;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, Result>
{
	private readonly IUnitOfWork _unitOfWork;

	public DeleteTenantCommandHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(
		DeleteTenantCommand request,
		CancellationToken cancellationToken)
	{
		var tenant = await _unitOfWork.Tenants
			.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

		if (tenant == null)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.Tenant.NotFound, "Tenant not found."));
		}

		// Aktif kullanıcı veya şirket kontrolü
		var hasUsers = await _unitOfWork.Users
			.AnyAsync(u => u.TenantId == request.Id, cancellationToken);

		if (hasUsers)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.Tenant.InvalidAccess, "Cannot delete tenant with active users."));
		}

		tenant.Deactivate();
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}