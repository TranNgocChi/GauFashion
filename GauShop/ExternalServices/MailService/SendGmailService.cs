using GauShop.ExternalServices.MailUtils;
using MimeKit;
using MailKit;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace GauShop.ExternalServices.MailService
{
    public class SendGmailService : ISendGmailService
    {
        private readonly MailSettings _mailSettings;

        private readonly ILogger<SendGmailService> logger;
        public SendGmailService(IOptions<MailSettings> mailSettings, ILogger<SendGmailService> _logger)
        {
            _mailSettings = mailSettings.Value;
            logger = _logger;
            logger.LogInformation("Create SendMailService");
        }
        public async Task<string> SendMail(MailContent mailContent)
        {
            var email = new MimeMessage();
            email.Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail);
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            email.To.Add(new MailboxAddress(mailContent.To, mailContent.To));
            email.Subject = mailContent.Subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = mailContent.Body;
            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
                await smtp.SendAsync(email);
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Error" + ex.Message;
            }
            smtp.Disconnect(true);
            return "Send Email Successfully!" ;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await SendMail(new MailContent()
            {
                To = email,
                Subject = subject,
                Body = htmlMessage
            });
        }
    }

    public class MailContent
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
