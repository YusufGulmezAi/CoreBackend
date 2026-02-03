using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Companies.Commands.Delete;

public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Result>
{
	private readonly IUnitOfWork _unitOfWork;

	public DeleteCompanyCommandHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(
		DeleteCompanyCommand request,
		CancellationToken cancellationToken)
	{
		var company = await _unitOfWork.Companies
			.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

		if (company == null)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.Company.NotFound, "Company not found."));
		}

		company.Delete();
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}