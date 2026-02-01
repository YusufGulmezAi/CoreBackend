using MediatR;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Auth.Commands.Register;

/// <summary>
/// Register command.
/// Yeni tenant ve admin kullanıcısı oluşturur.
/// </summary>
public record RegisterCommand(
	string TenantName,
	string Username,
	string Email,
	string Password,
	string ConfirmPassword,
	string FirstName,
	string LastName,
	string? Phone = null
) : IRequest<Result<RegisterResponse>>;