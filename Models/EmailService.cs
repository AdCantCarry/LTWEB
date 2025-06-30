// Services/EmailService.cs
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace TechNova.Models;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var emailSettings = _config.GetSection("EmailSettings");

        var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
        {
            Port = int.Parse(emailSettings["Port"]),
            Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(emailSettings["SenderEmail"], emailSettings["SenderName"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
