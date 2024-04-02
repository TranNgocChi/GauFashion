using BusinessObject.HomeViewModel;
using BusinessObject.Models;
using DataAccess.Repository.IObjectRepository;
using DataAccess.Repository.ObjectRepository;
using GauShop.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace GauShop.Controllers
{
    public class SignInController : Controller
    {
        //Register External Services
        private readonly SignInManager<User> _signInManager;
        private readonly IDistributedCache _sessionCache;
        private readonly SessionHelper _sessionHelper;

        // User Repository
        private readonly IUserRepository _userRepository = new UserRepository();

        //Cart Repository
        private readonly ICartRepository cartRepository = new CartRepository();

        //Inject External Services To Constructor
        public SignInController(SignInManager<User> signInManager, IDistributedCache sessionCache, SessionHelper sessionHelper)
        {
            _signInManager = signInManager;
            _sessionCache = sessionCache;
            _sessionHelper = sessionHelper;
        }

        [Route("/signin")]
        public async Task<IActionResult> ViewSignIn() => View("Views/Home/SignIn.cshtml");

        [HttpPost]
        public async Task<IActionResult> HandleSignIn(string email, string password)
        {
            if(_userRepository.IsEmailExists(email))
            {
                var user = _userRepository.GetByEmail(email);   
                if (user == null)
                {
                    return BadRequest("Error");
                }

                if(user.PasswordHash == null)
                {
                    TempData["ErrorMessage"] = "Email already logined by FB or GG. " +
                                               "Please select GG or FB to login ";
                    return RedirectToAction("ViewSignIn", "SignIn"); 
                }
            }

            if (_userRepository.AuthenticateUser(email, password))
            {
                //Get name from email
                var user = _userRepository.GetByEmail(email);
                if (user == null) { throw new Exception("User not found"); }

                //Create Session and Save to Redis
                var sessionId = user.Id;
                var sessionData = Encoding.UTF8.GetBytes(user.UserName);
                var sessionOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)//Time expires 1 day
                };
                await _sessionCache.SetAsync(sessionId, sessionData, sessionOptions);

                //Create Cookie and Send to User
                var cookieOption = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(1),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append("sessionId", sessionId, cookieOption);

                
                return RedirectToAction("Index", "Home");
            }
            TempData["ErrorMessage"] = "Email Not Match With Password!";
            return Redirect("/signin");
        }
            
        [HttpGet]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl = null)
        {
            // Kiểm tra yêu cầu dịch vụ provider tồn tại
            var providerFound = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList().FirstOrDefault();
            if (providerFound == null)
            {
                return NotFound("Dịch vụ không chính xác: " + provider);
            }

            var redirectUrl = Url.Action("ExternalLoginCallback", "SignIn", new { ReturnUrl = returnUrl });
            Console.WriteLine("returnUrl: " + returnUrl);

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                // Xử lý lỗi từ provider
                return Redirect("/signin"); // Hoặc hiển thị trang lỗi
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // Xử lý lỗi không lấy được thông tin xác thực
                return Redirect("/signin"); // Hoặc hiển thị trang lỗi
            }
            var user = info.Principal; // Thông tin người dùng

            var emailClaim = user.FindFirst(ClaimTypes.Email);
            var emailUser = emailClaim?.Value;

            var nameClaim = user.FindFirst(ClaimTypes.Name);
            var nameUser = nameClaim?.Value;

            string sessionId = "";
            byte[] sessionData = null;
            DistributedCacheEntryOptions sessionOptions = new DistributedCacheEntryOptions();
            CookieOptions cookieOption = new CookieOptions();

            //create user
            User foundUser = _userRepository.GetByEmail(emailUser);
            if (!_userRepository.IsEmailExists(emailUser))
            {
                User newUser = new User { UserName = nameUser, Email = emailUser };
                _userRepository.Create(newUser);
                //Create cart
                cartRepository.Create(new Cart { userId = newUser.Id, cartitems = new List<CartItem>() });
                //Create Session and Save to Redis
                sessionId = newUser.Id;
                sessionData = Encoding.UTF8.GetBytes(newUser.UserName);
                sessionOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)//Time expires 1 day
                };
                await _sessionCache.SetAsync(sessionId, sessionData, sessionOptions);
            }
            else
            {
                //Create Session and Save to Redis
                sessionId = foundUser.Id;
                sessionData = Encoding.UTF8.GetBytes(foundUser.UserName);
                sessionOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)//Time expires 1 day
                };
                await _sessionCache.SetAsync(sessionId, sessionData, sessionOptions);
            }

            //Create Cookie and Send to User
            cookieOption = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("sessionId", sessionId, cookieOption);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var sessionId = Request.Cookies["sessionId"];
            if (sessionId != null)
            {
                Response.Cookies.Delete("sessionId");
                await _sessionCache.RemoveAsync(sessionId);
            }
            return Redirect("/");
        }
    }
}
