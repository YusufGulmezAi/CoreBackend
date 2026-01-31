using CoreBackend.Application.Common.Models.Session;

namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Mevcut HTTP request'ten cihaz bilgilerini çýkarýr.
/// </summary>
public interface IDeviceInfoService
{
	/// <summary>
	/// Mevcut request'in cihaz bilgilerini getirir.
	/// </summary>
	DeviceInfo GetCurrentDeviceInfo();

	/// <summary>
	/// IP adresinden GeoLocation bilgisi getirir.
	/// </summary>
	Task<GeoLocation?> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
}