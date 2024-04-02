using BusinessObject.HomeViewModel;
using GauShop.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace GauShop.Controllers
{
    public class StoryController : Controller
    {
        private readonly SessionHelper _sessionHelper;

        public StoryController(SessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

        [Route("/story")]
        public async Task<IActionResult> ViewStory() {
            var model = await _sessionHelper.GetHomeModel();
            return View("Views/Home/Story.cshtml", model);
        }
    }
}
