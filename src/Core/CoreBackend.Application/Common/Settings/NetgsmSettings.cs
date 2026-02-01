namespace CoreBackend.Application.Common.Settings;

/// <summary>
/// Netgsm SMS ayarları.
/// </summary>
public class NetgsmSettings
{
	public const string SectionName = "NetgsmSettings";

	public string UserCode { get; set; } = null!;
	public string Password { get; set; } = null!;
	public string MessageHeader { get; set; } = null!;
}