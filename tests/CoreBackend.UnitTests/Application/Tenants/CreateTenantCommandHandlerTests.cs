using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Features.Tenants.Commands.Create;
using CoreBackend.Domain.Entities;
using CoreBackend.UnitTests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoreBackend.UnitTests.Application.Tenants;

/// <summary>
/// CreateTenantCommandHandler unit testleri.
/// </summary>
public class CreateTenantCommandHandlerTests : TestBase
{
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly CreateTenantCommandHandler _handler;

	public CreateTenantCommandHandlerTests()
	{
		_unitOfWorkMock = CreateMock<IUnitOfWork>();
		_handler = new CreateTenantCommandHandler(_unitOfWorkMock.Object);
	}

	#region Başarılı Senaryolar

	[Fact]
	public async Task Handle_YeniTenant_BasariylaOlusturulmali()
	{
		// Arrange
		var command = new CreateTenantCommand(
			Name: "Yeni Şirket",
			Email: "yeni@sirket.com",
			Phone: "+905551234567",
			Subdomain: "yeni-sirket",
			MaxCompanyCount: 10,
			SessionTimeoutMinutes: 60);

		// Mock: Veritabanında hiç tenant yok
		var bosListe = DbSetMockHelper.CreateEmptyMockDbSet<Tenant>();

		_unitOfWorkMock.Setup(x => x.Tenants).Returns(bosListe.Object);
		_unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(1);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue("Yeni tenant başarıyla oluşturulmalı");
		result.Value.Should().NotBeNull();
		result.Value.Name.Should().Be(command.Name);
		result.Value.Email.Should().Be(command.Email);

		_unitOfWorkMock.Verify(
			x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_SubdomainOlmadan_BasariylaOlusturulmali()
	{
		// Arrange
		var command = new CreateTenantCommand(
			Name: "Basit Şirket",
			Email: "basit@sirket.com",
			Phone: null,
			Subdomain: null,
			MaxCompanyCount: 5,
			SessionTimeoutMinutes: null);

		var bosListe = DbSetMockHelper.CreateEmptyMockDbSet<Tenant>();
		_unitOfWorkMock.Setup(x => x.Tenants).Returns(bosListe.Object);
		_unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(1);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Email.Should().Be(command.Email);
	}

	#endregion

	#region Email Çakışması Senaryoları

	[Fact]
	public async Task Handle_AyniEmail_HataDonmeli()
	{
		// Arrange
		var mevcutEmail = "mevcut@sirket.com";
		var command = new CreateTenantCommand(
			Name: "Yeni Şirket",
			Email: mevcutEmail);

		var mevcutTenant = Tenant.Create("Mevcut Şirket", mevcutEmail);
		var tenantListesi = DbSetMockHelper.CreateMockDbSet(
			new List<Tenant> { mevcutTenant }.AsQueryable());

		_unitOfWorkMock.Setup(x => x.Tenants).Returns(tenantListesi.Object);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue("Duplicate email reddedilmeli");
		result.Error.Code.Should().Be("TENANT_ALREADY_EXISTS");

		_unitOfWorkMock.Verify(
			x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Never);
	}

	#endregion

	#region Subdomain Çakışması Senaryoları

	[Fact]
	public async Task Handle_AyniSubdomain_HataDonmeli()
	{
		// Arrange
		var mevcutSubdomain = "mevcut-sirket";
		var command = new CreateTenantCommand(
			Name: "Yeni Şirket",
			Email: "yeni@sirket.com",
			Subdomain: mevcutSubdomain);

		var mevcutTenant = Tenant.Create("Mevcut Şirket", "mevcut@sirket.com");
		mevcutTenant.Subdomain = mevcutSubdomain;

		var tenantListesi = DbSetMockHelper.CreateMockDbSet(
			new List<Tenant> { mevcutTenant }.AsQueryable());

		_unitOfWorkMock.Setup(x => x.Tenants).Returns(tenantListesi.Object);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue("Duplicate subdomain reddedilmeli");
		result.Error.Code.Should().Be("TENANT_ALREADY_EXISTS");

		_unitOfWorkMock.Verify(
			x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Never);
	}

	#endregion

	#region Varsayılan Değer Testleri

	[Fact]
	public async Task Handle_MinimumParametreler_VarsayilanDegerlerDogru()
	{
		// Arrange
		var command = new CreateTenantCommand(
			Name: "Minimal Şirket",
			Email: "minimal@sirket.com");

		var bosListe = DbSetMockHelper.CreateEmptyMockDbSet<Tenant>();
		_unitOfWorkMock.Setup(x => x.Tenants).Returns(bosListe.Object);
		_unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(1);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.MaxCompanyCount.Should().Be(5, "Varsayılan MaxCompanyCount 5 olmalı");
	}

	#endregion
}