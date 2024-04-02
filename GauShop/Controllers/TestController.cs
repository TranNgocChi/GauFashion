using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace GauShop.Controllers
{
    public class TestController : Controller
    {
        private readonly IDistributedCache _distributeCache;

        public TestController(IDistributedCache distributeCache)
        {
            _distributeCache = distributeCache;
        }
        public IActionResult TestRedis()
        {
            _distributeCache.SetString("sessionId", "Data");
            var data = _distributeCache.GetString("sessionId");
            return Content(data);
        }
    }
}
