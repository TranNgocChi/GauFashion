namespace GauShop.ExternalServices.MailService
{
    public interface ISendGmailService
    {
        Task<string> SendMail(MailContent mailContent);

        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
