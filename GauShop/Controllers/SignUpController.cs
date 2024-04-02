using BusinessObject.Models;
using DataAccess.PasswordHash;
using DataAccess.Repository.IObjectRepository;
using DataAccess.Repository.ObjectRepository;
using GauShop.ExternalServices.MailService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace GauShop.Controllers
{
    public class SignUpController : Controller
    {
        //Register External Services
        private readonly IDistributedCache _sessionCache;
        private readonly ISendGmailService _sendGmailService;

        private readonly IUserRepository userRepository = new UserRepository();
        private readonly ICartRepository cartRepository = new CartRepository();
        private readonly IPasswordHasherService _passwordHasher = new PasswordHasherService();

        public SignUpController(ISendGmailService sendGmailService, IDistributedCache sessionCache)
        {
            _sendGmailService = sendGmailService;
            _sessionCache = sessionCache;
        }

        [Route("/signup")]
        public IActionResult ViewSignUp() => View("Views/Home/SignUp.cshtml");

        [HttpPost]
        public async Task<IActionResult> HandleSignUp(string name, string email, string password)
        {
            if (userRepository.IsEmailExists(email))
            {
                TempData["ErrorMessage"] = "Email Existed!";
                return Redirect("/signup");
            }
            else
            {
                string authUrl = $"https://localhost:7192/track-link";
                MailContent content = new MailContent
                {
                    To = email,
                    Subject = "Authentication Login From GauShop",
                    Body = $@"
                    <!DOCTYPE html>
                    <html lang='en'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Account Verification</title>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                margin: 0;
                                padding: 0;
                            }}
                            .container {{
                                max-width: 600px;
                                margin: 20px auto;
                                padding: 20px;
                                background-color: #fff;
                                border-radius: 8px;
                                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                            }}
                            h2 {{
                                color: #333;
                            }}
                            p {{
                                margin-bottom: 20px;
                            }}
                            button {{
                                color: #fff;
                                text-decoration: none;
                                background-color: #007bff;
                                padding: 10px 20px;
                                border-radius: 5px;
                                border: none;
                                cursor: pointer;
                            }}
                            button:hover {{
                                background-color: #0056b3;
                            }}
                        </style>
                        </head>
                        <body>
                            <div class='container'>
                                <h2>Hello {name},</h2>
                                <p>Thank you for registering an account. Please click the button below to verify your account:</p>
                                <form action='{authUrl}' method='GET'>
                                    <input type='hidden' name='userName' value='{name}' />
                                    <input type='hidden' name='email' value='{email}' />
                                    <input type='hidden' name='password' value='{password}' />
                                    <button type='submit'>Verify Account</button>
                                </form>
                                <p>If you did not request this verification, please disregard this email.</p>
                                <p>Best regards,</p>
                                <p>Our support team</p>
                            </div>
                        </body>
                        </html>"
                };

                await _sendGmailService.SendMail(content);
                TempData["notification"] = $"Email Sent To {email}. Please check on your email!";
                return Redirect("/signup");
            }
        }

        [HttpGet("/track-link")]
        public async Task<IActionResult> TrackLink(string userName, string email, string password)
        {
            try
            {
                if (userName != null && email != null && !userRepository.IsEmailExists(email))
                {
                    //Create new user in mongodb
                    User newUser = new User { UserName = userName, Email = email, PasswordHash = _passwordHasher.HashPassword(password), EmailConfirmed = true };
                    userRepository.Create(newUser);

                    //Create cart
                    cartRepository.Create(new Cart { userId = newUser.Id, cartitems = new List<CartItem>()});

                    TempData["notification"] = $"Authenticate Successfully! Please Login Again!";

                    return Redirect("/signin");
                }
                return Content("You verified so this link invalid Or Error Occured!");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
