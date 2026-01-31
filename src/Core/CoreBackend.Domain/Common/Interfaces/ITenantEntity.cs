namespace CoreBackend.Domain.Common.Interfaces;

/// <summary>
/// Tenant'a (mali müşavir/muhasebeci) bağlı entity'ler için interface.
/// Multi-tenancy desteği sağlar.
/// </summary>
public interface ITenantEntity
{
	Guid TenantId { get; }
}