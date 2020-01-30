using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace UofUStudentVerificationBot
{
    public class EmailService : IEmailService
    {
        private readonly LogService logService;
        private readonly IConfiguration config;

        public EmailService(LogService logService, IConfiguration config)
        {
            this.logService = logService;
            this.config = config;
        }

        public async Task SendEmail(string uID, string verificationCode)
        {
            using (SmtpClient smtpClient = new SmtpClient(config["Smtp:Host"], config.GetValue<int>("Smtp:Port")))
            using (MailMessage mailMessage = new MailMessage()) {
                smtpClient.Timeout = 10000;  // 10 seconds
                smtpClient.Credentials = new NetworkCredential(config["Smtp:Username"], config["Smtp:Password"]);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.EnableSsl = true;
                mailMessage.From = new MailAddress(config["Smtp:Username"]);  
                mailMessage.To.Add(new MailAddress($"{uID}@umail.utah.edu"));
                mailMessage.Subject = "discord verification code";
                mailMessage.Body = $"if you requested verification from the UofU Student Verification Discord Bot, then reply to the bot's DM with this code:\n\n{verificationCode}\n\nif this wasn't you, then you can safely ignore and delete this email.";
                await logService.LogInfo("EmailService", $"sending verification email to {uID}@umail.utah.edu...");
                smtpClient.Send(mailMessage);
            }
        }
    }
}