using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotificationService.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var sendgrdApiKey = _configuration["SendGridSettings:ApiKey"];
        var senderEmail = _configuration["SendGridSettings:FromEmail"];
        var senderName = _configuration["SendGridSettings:FromName"];


        var client = new SendGridClient(sendgrdApiKey);
        var from = new EmailAddress(senderEmail, senderName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);
        var response = await client.SendEmailAsync(msg);

        if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
        {
            // Handle error
            var responseBody = await response.Body.ReadAsStringAsync();
            throw new Exception($"Failed to send email: {response.StatusCode}, {responseBody}");
        }
    }
}