using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Companies.Commands.Delete;

public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Result>
{
	private readonly IRepositoryExtended<Company, Guid> _companyRepository;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteCompanyCommandHandler(
		IRepositoryExtended<Company, Guid> companyRepository,
		IUnitOfWork unitOfWork)
	{
		_companyRepository = companyRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(
		DeleteCompanyCommand request,
		CancellationToken cancellationToken)
	{
		var company = await _companyRepository.GetByIdAsync(request.Id, cancellationToken);

		if (company == null)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.Company.NotFound, "Company not found."));
		}

		company.Delete();
		_companyRepository.Update(company);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}