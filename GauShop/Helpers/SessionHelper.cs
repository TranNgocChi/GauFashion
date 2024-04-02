using BusinessObject.HomeViewModel;
using BusinessObject.Models;
using DataAccess.Repository.IObjectRepository;
using DataAccess.Repository.ObjectRepository;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace GauShop.Helpers
{
    public class SessionHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _sessionCache;
        
        private readonly ICartRepository _cartRepository = new CartRepository();
        private readonly IUserRepository _userRepository = new UserRepository();
        public SessionHelper(IHttpContextAccessor httpContextAccessor, IDistributedCache sessionCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _sessionCache = sessionCache;
        }

        public async Task<HomeModel> GetHomeModel()
        {
            User user = new User();
            var sessionID = _httpContextAccessor.HttpContext.Request.Cookies["sessionId"];
            if (!string.IsNullOrEmpty(sessionID))
            {
                user = _userRepository.GetById(sessionID);
                if (user == null)
                {
                    _cartRepository.Delete(new Cart { userId = sessionID });
                    _httpContextAccessor.HttpContext.Response.Cookies.Delete("sessionId");
                    await _sessionCache.RemoveAsync(sessionID);
                    return new HomeModel { sessionId = null, sessionName = null };
                }
                var sessionName = await _sessionCache.GetAsync(sessionID);
                if (sessionName != null)
                {
                    var myCart = _cartRepository.GetByUserId(sessionID);
                    myCart.total = myCart.cartitems
                    .Where(item => item.selected == true)
                    .Sum(item => item.quantity * item.product.price);

                    var username = Encoding.UTF8.GetString(sessionName);
                    return new HomeModel { sessionId = sessionID, sessionName = username, cart = myCart };
                }
            }
            return new HomeModel { sessionId = null, sessionName = null };
        }
    }
}
