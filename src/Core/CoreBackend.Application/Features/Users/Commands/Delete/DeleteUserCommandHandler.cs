using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Users.Commands.Delete;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
	private readonly IRepositoryExtended<User, Guid> _userRepository;
	private readonly ICurrentUserService _currentUserService;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteUserCommandHandler(
		IRepositoryExtended<User, Guid> userRepository,
		ICurrentUserService currentUserService,
		IUnitOfWork unitOfWork)
	{
		_userRepository = userRepository;
		_currentUserService = currentUserService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(
		DeleteUserCommand request,
		CancellationToken cancellationToken)
	{
		// Kendini silmeyi engelle
		if (_currentUserService.UserId == request.Id)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.User.CannotDeleteSelf, "You cannot delete yourself."));
		}

		var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

		if (user == null)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.User.NotFound, "User not found."));
		}

		user.Delete();
		_userRepository.Update(user);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}