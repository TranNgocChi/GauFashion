using Amazon.Runtime.SharedInterfaces;
using BusinessObject.HomeViewModel;
using GauShop.ExternalServices.MailService;
using GauShop.ExternalServices.MailUtils;
using GauShop.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Mozilla;

namespace GauShop.Controllers
{
    public class ContactController : Controller
    {
        private readonly MailSettings _mailSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly ISendGmailService _sendGmailService;

        public ContactController(SessionHelper sessionHelper, ISendGmailService sendGmailService, IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
            _sendGmailService = sendGmailService;
            _sessionHelper = sessionHelper;
        }
        [Route("/contact")]
        public async Task<IActionResult> ViewContact()
        {
            var model = await _sessionHelper.GetHomeModel();
            return View("Views/Home/Contact.cshtml", model);
        }

        public IActionResult SendRequirement(string name, string email, string subject, string message)
        {
            MailContent content = new MailContent
            {
                To = _mailSettings.Mail,
                Subject = subject,
                Body = $"<h2>Mail from {email}</h2>" +
                $"<p style='font-size: 25px;'> My name is <strong>{name}</strong> </p>" +
                $"<p style='font-size: 15px;'>{message}</p>"
            };
            _sendGmailService.SendMail(content);
            return RedirectToAction("ViewContact", "Contact");
        }
    }
}
