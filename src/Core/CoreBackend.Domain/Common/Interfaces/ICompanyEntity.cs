namespace CoreBackend.Domain.Common.Interfaces;

/// <summary>
/// Şirkete bağlı entity'ler için interface.
/// Mali müşavirin yönettiği şirketleri ayırt eder.
/// </summary>
public interface ICompanyEntity
{
	Guid CompanyId { get; }
}