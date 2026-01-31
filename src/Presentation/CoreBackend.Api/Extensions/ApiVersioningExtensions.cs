using Asp.Versioning;

namespace CoreBackend.Api.Extensions;

/// <summary>
/// API versiyonlama extension metodları.
/// </summary>
public static class ApiVersioningExtensions
{
	/// <summary>
	/// API versiyonlamayı yapılandırır.
	/// </summary>
	public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
	{
		services.AddApiVersioning(options =>
		{
			// Varsayılan versiyon
			options.DefaultApiVersion = new ApiVersion(1, 0);

			// Versiyon belirtilmezse varsayılanı kullan
			options.AssumeDefaultVersionWhenUnspecified = true;

			// Response header'da desteklenen versiyonları göster
			options.ReportApiVersions = true;

			// Versiyon okuma kaynakları (Hibrit: URL + Header)
			options.ApiVersionReader = ApiVersionReader.Combine(
				new UrlSegmentApiVersionReader(),
				new HeaderApiVersionReader("X-Api-Version")
			);
		});

		return services;
	}
}