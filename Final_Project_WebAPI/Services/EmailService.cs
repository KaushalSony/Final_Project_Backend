using Final_Project_WebAPI;
using Final_Project_WebAPI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly IConfiguration _configuration;


    public EmailService(IOptions<SmtpSettings> smtpSettings, IConfiguration configuration)
    {
        _smtpSettings = smtpSettings.Value;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Read from environment variables (works with user-secrets in .NET)

        var username = _configuration["SMTP:USERNAME"]
            ?? throw new InvalidOperationException("SMTP__USERNAME is not set in environment variables or user-secrets.");
        var password = _configuration["SMTP:PASSWORD"]
            ?? throw new InvalidOperationException("SMTP__PASSWORD is not set in environment variables or user-secrets.");

        try
        {
            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_smtpSettings.From),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
            Console.WriteLine($"Email sent to {to} successfully.");
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine($"SMTP error: {smtpEx.StatusCode} - {smtpEx.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EmailService error: {ex.Message}");
            throw;
        }
    }
}