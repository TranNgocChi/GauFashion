using BusinessObject.HomeViewModel;
using BusinessObject.Models;
using DataAccess.Repository.IObjectRepository;
using DataAccess.Repository.ObjectRepository;
using GauShop.ExternalServices.MailService;
using GauShop.Helpers;
using GauShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
using System.Text;

namespace GauShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SessionHelper _sessionHelper;
        private readonly IDistributedCache _sessionCache;

        private readonly IUserRepository _userRepository = new UserRepository();
        private readonly ICartRepository _cartRepository = new CartRepository();
        public HomeController(ILogger<HomeController> logger, SessionHelper sessionHelper, IDistributedCache sessionCache)
        {
            _sessionHelper = sessionHelper;
            _logger = logger;
            _sessionCache = sessionCache;
        }

        [Route("/")]
        public async Task<IActionResult> Index()
        {
            var model = await _sessionHelper.GetHomeModel();
            return View(model);
        } 

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult TestSession()
        {
            int? count;
            count = HttpContext.Session.GetInt32("count");
            if (count == null)
            {
                count = 0;
            }
            count += 1;
            HttpContext.Session.SetInt32("count", count.Value);

            return Content("So lan truy cap: " + count);
        }

        public IActionResult Check() => View();
        
    }
}


