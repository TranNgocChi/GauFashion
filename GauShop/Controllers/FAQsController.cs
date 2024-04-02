using BusinessObject.HomeViewModel;
using Microsoft.AspNetCore.Mvc;

namespace GauShop.Controllers
{
    public class FAQsController : Controller
    {
        [Route("/faq")]
        public IActionResult ViewFaqs() => View("Views/Home/Faq.cshtml", new HomeModel { sessionId = Request.Cookies["sessionId"] });
    }
}
