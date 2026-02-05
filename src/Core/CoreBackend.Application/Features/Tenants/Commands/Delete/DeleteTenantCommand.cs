using MediatR;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Tenants.Commands.Delete;

public record DeleteTenantCommand(Guid Id) : IRequest<Result>;