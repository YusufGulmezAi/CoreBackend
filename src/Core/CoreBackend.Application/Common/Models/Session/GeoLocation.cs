namespace CoreBackend.Application.Common.Models.Session;

/// <summary>
/// Coðrafi konum bilgisi.
/// </summary>
public sealed record GeoLocation
{
	/// <summary>
	/// Ülke kodu (TR, US, DE vb.).
	/// </summary>
	public required string CountryCode { get; init; }

	/// <summary>
	/// Ülke adý.
	/// </summary>
	public string? CountryName { get; init; }

	/// <summary>
	/// Þehir.
	/// </summary>
	public string? City { get; init; }

	/// <summary>
	/// Bölge/Eyalet.
	/// </summary>
	public string? Region { get; init; }

	/// <summary>
	/// Enlem.
	/// </summary>
	public double? Latitude { get; init; }

	/// <summary>
	/// Boylam.
	/// </summary>
	public double? Longitude { get; init; }

	/// <summary>
	/// Zaman dilimi.
	/// </summary>
	public string? TimeZone { get; init; }

	/// <summary>
	/// Ýki konum arasý mesafe (km).
	/// Ýmkansýz seyahat tespiti için kullanýlýr.
	/// </summary>
	public double? DistanceKmFrom(GeoLocation other)
	{
		if (Latitude is null || Longitude is null ||
			other.Latitude is null || other.Longitude is null)
			return null;

		// Haversine formula
		const double EarthRadiusKm = 6371;

		var lat1Rad = Latitude.Value * Math.PI / 180;
		var lat2Rad = other.Latitude.Value * Math.PI / 180;
		var deltaLatRad = (other.Latitude.Value - Latitude.Value) * Math.PI / 180;
		var deltaLonRad = (other.Longitude.Value - Longitude.Value) * Math.PI / 180;

		var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
				Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
				Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

		var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

		return EarthRadiusKm * c;
	}

	/// <summary>
	/// Ýmkansýz seyahat kontrolü.
	/// Belirtilen sürede bu mesafenin kat edilip edilemeyeceðini kontrol eder.
	/// </summary>
	/// <param name="other">Diðer konum</param>
	/// <param name="timeDifference">Ýki konum arasýndaki zaman farký</param>
	/// <param name="maxSpeedKmh">Maksimum kabul edilebilir hýz (varsayýlan: 1000 km/h - uçak hýzý)</param>
	public bool IsImpossibleTravel(GeoLocation other, TimeSpan timeDifference, double maxSpeedKmh = 1000)
	{
		var distance = DistanceKmFrom(other);
		if (distance is null) return false;

		var hours = timeDifference.TotalHours;
		if (hours <= 0) return distance > 0;

		var requiredSpeed = distance.Value / hours;
		return requiredSpeed > maxSpeedKmh;
	}
}