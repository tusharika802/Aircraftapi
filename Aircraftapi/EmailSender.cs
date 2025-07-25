using Aircraftapi;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;

namespace Aircraftapi
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly EmailSettings _emailSettings;

        public EmailSender(IConfiguration configuration, IOptions<EmailSettings> emailSettings)
        {
            _configuration = configuration;
            _emailSettings = emailSettings.Value;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }
        public async Task Execute(string email, string subject, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    email = _emailSettings.ToEmail;
                }

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.UsernameEmail, "HealthCare System"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                    Priority = MailPriority.High
                };

                mailMessage.To.Add(email);
                if (!string.IsNullOrEmpty(_emailSettings.CcEmail))
                {
                    mailMessage.CC.Add(_emailSettings.CcEmail);
                }

                using var smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(mailMessage);
                Console.WriteLine($"✅ Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send email: {ex.Message}");
            }
        }
    }
}

