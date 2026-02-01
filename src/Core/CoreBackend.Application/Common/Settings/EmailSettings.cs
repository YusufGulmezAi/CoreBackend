namespace CoreBackend.Application.Common.Settings;

/// <summary>
/// Email ayarları.
/// </summary>
public class EmailSettings
{
	public const string SectionName = "EmailSettings";

	public string SmtpHost { get; set; } = null!;
	public int SmtpPort { get; set; } = 587;
	public string SmtpUsername { get; set; } = null!;
	public string SmtpPassword { get; set; } = null!;
	public bool UseSsl { get; set; } = true;
	public string FromEmail { get; set; } = null!;
	public string FromName { get; set; } = null!;
}