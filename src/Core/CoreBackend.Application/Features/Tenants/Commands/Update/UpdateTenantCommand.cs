using MediatR;
using CoreBackend.Contracts.Tenants.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Tenants.Commands.Update;

public record UpdateTenantCommand(
	Guid Id,
	string Name,
	string Email,
	string? Phone = null,
	string? Subdomain = null,
	string? ContactEmail = null,
	string? ContactPhone = null,
	int? MaxCompanyCount = null,
	int? SessionTimeoutMinutes = null
) : IRequest<Result<TenantResponse>>;