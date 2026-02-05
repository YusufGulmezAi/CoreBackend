using MediatR;
using CoreBackend.Contracts.Tenants.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Tenants.Queries.GetById;

public record GetTenantByIdQuery(Guid Id) : IRequest<Result<TenantResponse>>;