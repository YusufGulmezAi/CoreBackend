using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Settings;

namespace CoreBackend.Infrastructure.Services;

/// <summary>
/// Email servis implementasyonu.
/// </summary>
public class EmailService : IEmailService
{
	private readonly EmailSettings _settings;
	private readonly ILogger<EmailService> _logger;

	public EmailService(
		IOptions<EmailSettings> settings,
		ILogger<EmailService> logger)
	{
		_settings = settings.Value;
		_logger = logger;
	}

	public async Task<bool> SendAsync(
		string to,
		string subject,
		string body,
		bool isHtml = true,
		CancellationToken cancellationToken = default)
	{
		try
		{
			using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
			{
				Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
				EnableSsl = _settings.UseSsl
			};

			var message = new MailMessage
			{
				From = new MailAddress(_settings.FromEmail, _settings.FromName),
				Subject = subject,
				Body = body,
				IsBodyHtml = isHtml
			};
			message.To.Add(to);

			await client.SendMailAsync(message, cancellationToken);

			_logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send email to {To}", to);
			return false;
		}
	}

	public async Task<bool> SendTwoFactorCodeAsync(
		string to,
		string code,
		int expirationMinutes,
		CancellationToken cancellationToken = default)
	{
		var subject = "Your verification code";
		var body = $@"
            <html>
            <body>
                <h2>Your verification code</h2>
                <p>Your verification code is: <strong>{code}</strong></p>
                <p>This code will expire in {expirationMinutes} minutes.</p>
                <p>If you didn't request this code, please ignore this email.</p>
            </body>
            </html>";

		return await SendAsync(to, subject, body, true, cancellationToken);
	}

	public async Task<bool> SendPasswordResetAsync(
		string to,
		string resetLink,
		CancellationToken cancellationToken = default)
	{
		var subject = "Password Reset Request";
		var body = $@"
            <html>
            <body>
                <h2>Password Reset</h2>
                <p>Click the link below to reset your password:</p>
                <p><a href='{resetLink}'>{resetLink}</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request this, please ignore this email.</p>
            </body>
            </html>";

		return await SendAsync(to, subject, body, true, cancellationToken);
	}
}