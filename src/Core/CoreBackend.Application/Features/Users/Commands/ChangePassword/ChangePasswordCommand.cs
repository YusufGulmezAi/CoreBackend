using MediatR;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Users.Commands.ChangePassword;

public record ChangePasswordCommand(
	string CurrentPassword,
	string NewPassword,
	string ConfirmNewPassword
) : IRequest<Result>;