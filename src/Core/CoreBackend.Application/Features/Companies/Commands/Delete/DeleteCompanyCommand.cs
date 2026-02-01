using MediatR;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Companies.Commands.Delete;

public record DeleteCompanyCommand(Guid Id) : IRequest<Result>;