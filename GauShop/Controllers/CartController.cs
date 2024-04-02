using BusinessObject.Models;
using DataAccess.Repository.IObjectRepository;
using DataAccess.Repository.ObjectRepository;
using GauShop.Helpers;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Mozilla;
using System.Net;
using ZstdSharp.Unsafe;

namespace GauShop.Controllers
{
    
    public class CartController : Controller
    {
        private readonly SessionHelper _sessionHelper;
        private readonly ICartRepository _cartRepository = new CartRepository();
        private readonly IProductRepository _productRepository = new ProductRepository();
        public CartController(SessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

        [Route("/cart")]
        public async Task<IActionResult> ViewCart()
        {
            var model = await _sessionHelper.GetHomeModel();

            model.cartItems = model.cart.cartitems;

            return View("Views/Home/Cart.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string id, string size, string color, string productQuantity)
        {
            var model = await _sessionHelper.GetHomeModel();
            if (model.sessionId == null)
            {
                TempData["ErrorMessage"] = "Please Login Before You Add To Cart";
                return RedirectToAction("ViewSignIn","SignIn");
            }
            //session id chinh la Id User loai bo tien to
            var sessionIdWithoutPrefix = model.sessionId.Replace("Session", "");

            //Tim cart dua vao User ID
            Cart myCart = _cartRepository.GetByUserId(sessionIdWithoutPrefix);

            if (id == null)
            {
                return Content("Product not found");
            }

            if (size == null || color == null)
            {
                return Content("null");
            }

            //foundProduct by id
            var foundProduct = _productRepository.GetById(id);

            bool isCartItemExists = myCart.cartitems.Any(
                item => item.product.id == foundProduct.id
                && item.color == color
                && item.size == size    
            );
            
            if (isCartItemExists)
            {
                var foundCartItem = myCart.cartitems.Where(c => c.product.id == foundProduct.id
                                                        && c.size == size
                                                        && c.color == color)
                                                    .FirstOrDefault();
                if(foundCartItem == null)
                {
                    return Content("Error Occured");
                }
                foundCartItem.quantity += int.Parse(productQuantity);


            }
            else
            {
                myCart.cartitems.Add(new CartItem { product = foundProduct, color = color
                                    , size = size, quantity = int.Parse(productQuantity) });
            
            }

            _cartRepository.Update(myCart, myCart.id);
            return Redirect("/cart");
        }

        [HttpGet]
        [Route("/remove-item")]
        public async Task<IActionResult> RemoveItem(string idProduct, string sizeProduct, string colorProduct)
        {
            if(idProduct == null || sizeProduct == null || colorProduct == null)
            {
                return Content("Error");
            }

            var model = await _sessionHelper.GetHomeModel();
            if (model.sessionId == null)
            {
                TempData["ErrorMessage"] = "Please Login Before You Add To Cart";
                return RedirectToAction("ViewSignIn", "SignIn");
            }
            //session id chinh la Id User loai bo tien to
            var sessionIdWithoutPrefix = model.sessionId.Replace("Session", "");

            //Tim cart dua vao User ID
            Cart myCart = _cartRepository.GetByUserId(sessionIdWithoutPrefix);

            if(myCart == null)
            {
                TempData["ErrorMessage"] = "Please Login Before You Add To Cart";
                return RedirectToAction("ViewSignIn", "SignIn");
            }

            //found cartitem
            var cartitemRemove = myCart.cartitems.Where(ct => ct.product.id == idProduct 
                                                && ct.size == sizeProduct
                                                && ct.color == colorProduct
                                                ).FirstOrDefault();

            if(cartitemRemove == null)
            {
                return Content("Error Ocurred!");
            }

            myCart.cartitems.Remove(cartitemRemove);

            _cartRepository.Update(myCart, myCart.id);

            return RedirectToAction("ViewCart", "Cart");
        }

        [HttpPost]
        [Route("/update-selected-products")]
        public async Task<IActionResult> UpdateSelectedProducts(List<string> selectedProducts)
        {
            var model = await _sessionHelper.GetHomeModel();

            if(model.sessionId == null)
            {
                TempData["ErrorMessage"] = "Please Login!";
                return RedirectToAction("ViewSignIn", "SignIn");
            }

            Cart cart = _cartRepository.GetByUserId(model.sessionId);
            if (cart == null)
            {
                TempData["ErrorMessage"] = "Please Login!";
                return RedirectToAction("ViewSignIn", "SignIn");
            }

            if(selectedProducts == null)
            {
                return Content("ko co san pham duoc chon") ;
            }
            
            foreach (var stProduct in selectedProducts)
            {
                var selectedCartItem = cart.cartitems.FirstOrDefault(ct => ct.product.id+ct.size+ct.color == stProduct);
                if (selectedCartItem == null)
                {
                    return Content("Null");
                }
                selectedCartItem.selected = true;
            }
            cart.total = cart.cartitems
                    .Where(item => item.selected == true)
                    .Sum(item => item.quantity * item.product.price);
            _cartRepository.Update(cart, cart.id);
            return Json(new { cart = cart });
        }

        [HttpPost]
        [Route("/update-notSelected-products")]
        public async Task<IActionResult> UpdateNotSelectedProducts(List<string> notSelectedProducts)
        {
            var model = await _sessionHelper.GetHomeModel();

            Cart cart = _cartRepository.GetByUserId(model.sessionId);
            if (cart == null)
            {
                return Content("Error Occured!");
            }

            if (notSelectedProducts == null)
            {
                return Content("ko co san pham duoc chon");
            }

            foreach (var stProduct in notSelectedProducts)
            {
                var notSelectedCartItem = cart.cartitems.FirstOrDefault(ct => ct.product.id + ct.size + ct.color == stProduct);
                if (notSelectedCartItem == null)
                {
                    return Content("Null");
                }
                notSelectedCartItem.selected = false;
            }
            cart.total = cart.cartitems
                    .Where(item => item.selected == true)
                    .Sum(item => item.quantity * item.product.price);
            _cartRepository.Update(cart, cart.id);
            return Json(new { cart = cart });
        }

        [HttpPost]
        [Route("/update-quantity")]
        public async Task<IActionResult> UpdateQuantityInCart(string productId, string action)
        {
            var model = await _sessionHelper.GetHomeModel();

            if (model.sessionId == null)
            {
                TempData["ErrorMessage"] = "Please Login!";
                return RedirectToAction("ViewSignIn", "SignIn");
            }

            Cart cart = model.cart;
            if(cart == null)
            {
                TempData["ErrorMessage"] = "Please Login!";
                return RedirectToAction("ViewSignIn", "SignIn");
            }

            var cartitem = cart.cartitems.FirstOrDefault(item => item.product.id+item.size+item.color == productId);

            if (cartitem == null)
            {
                //
            }

            if(action == "increase")
            {
                cartitem.quantity++;
                
            }
            else if(action == "decrease")
            {
                if(cartitem.quantity > 1)
                {
                    cartitem.quantity--;
                }
                
            }
            else
            {
                return Content("Error");
            }

            cart.total = cart.cartitems
                    .Where(item => item.selected == true)
                    .Sum(item => item.quantity * item.product.price);
            
            _cartRepository.Update(cart, cart.id);
            var cartitemUpdated = cart.cartitems.FirstOrDefault(item => item.product.id + item.size + item.color == productId);

            return Json(new {cartitem = cartitemUpdated, cart=cart });
        }
    }
}
