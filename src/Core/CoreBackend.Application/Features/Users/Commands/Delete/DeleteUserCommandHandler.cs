using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Users.Commands.Delete;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly ICurrentUserService _currentUserService;

	public DeleteUserCommandHandler(
		IUnitOfWork unitOfWork,
		ICurrentUserService currentUserService)
	{
		_unitOfWork = unitOfWork;
		_currentUserService = currentUserService;
	}

	public async Task<Result> Handle(
		DeleteUserCommand request,
		CancellationToken cancellationToken)
	{
		if (_currentUserService.UserId == request.Id)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.User.CannotDeleteSelf, "You cannot delete yourself."));
		}

		var user = await _unitOfWork.Users
			.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

		if (user == null)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.User.NotFound, "User not found."));
		}

		user.Delete();
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}