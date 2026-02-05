using MediatR;
using CoreBackend.Contracts.Tenants.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Tenants.Commands.Create;

public record CreateTenantCommand(
	string Name,
	string Email,
	string? Phone = null,
	string? Subdomain = null,
	int MaxCompanyCount = 5,
	int? SessionTimeoutMinutes = null
) : IRequest<Result<TenantResponse>>;