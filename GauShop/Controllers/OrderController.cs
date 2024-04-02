using BusinessObject.Models;
using BusinessObject.VNProvince;
using DataAccess.Repository.IObjectRepository;
using DataAccess.Repository.ObjectRepository;
using GauShop.ExternalServices.MailService;
using GauShop.ExternalServices.MailUtils;
using GauShop.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Xml.Schema;
using BusinessObject.Admin;
using GauShop.ExternalServices.VnPayService;
using Org.BouncyCastle.Asn1.Mozilla;
using Microsoft.Extensions.Caching.Distributed;

namespace GauShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly SessionHelper _sessionHelper;
		private readonly IDistributedCache _sessionCache;

		private readonly ICartRepository _cartRepository = new CartRepository();
        private readonly IOrderRepository _orderRepository = new OrderRepository();
        private readonly IUserRepository _userRepository = new UserRepository();

		private readonly ISendGmailService _sendGmailService;
		private readonly MailSettings _mailSettings;
        private readonly IVnPayService _vnPayService;

		//Get provinces from db
		private readonly VietnameseAdministrativeUnitsContext _vnContext = new VietnameseAdministrativeUnitsContext();
		public OrderController(SessionHelper sessionHelper, ISendGmailService sendGmailService,
			IOptions<MailSettings> mailSettings, IVnPayService vnPayService
            ,IDistributedCache sessionCache)
        {
            _sessionHelper = sessionHelper;
			_sendGmailService = sendGmailService;
			_mailSettings = mailSettings.Value;
			_vnPayService = vnPayService;  
            _sessionCache = sessionCache;
		}

        //Handle View Order
        [Route("/order")]
        [HttpPost]
        public async Task<IActionResult> ViewOrder()
        {
            //Model got from session
            var model = await _sessionHelper.GetHomeModel();
            if(model.sessionId == null || model.sessionName == null)
            {
                TempData["ErrorMessage"] = "Please Login";
                return RedirectToAction("ViewSignIn", "SignIn");
            }


            var cart = _cartRepository.GetByUserId(model.sessionId);
            if (cart== null)
            {
                TempData["ErrorMessage"] = "Please Login";
                return RedirectToAction("ViewSignIn", "SignIn");
            }

            //Handle Order With Products From Cart
            model.cartOrder = cart;

            List<CartItem> itemsToRemoves = new List<CartItem>();

            foreach (var item in cart.cartitems)
            {
                if (!item.selected)
                {
                    itemsToRemoves.Add(item);
                }
            }

            foreach (var itemToRemove in itemsToRemoves)
            {
                model.cartOrder.cartitems.Remove(itemToRemove);
            }

            decimal total = 0;
            if (model.cartOrder.cartitems.Count <= 0)
            {
                return Content("Ban chua chon san pham");
            }
            foreach(var ct in model.cartOrder.cartitems)
            {
                total += ct.quantity * ct.product.price;
            }

            model.cartOrder.total = total;

			//Display province, district, ward

            //create object vnMap
			model.vNMap = new VNMap();

			//Get All Provinces
			var provinces = _vnContext.Provinces.ToList();
            if(provinces.Count > 0)
            {
				model.vNMap.provinces = _vnContext.Provinces.ToList();
			}
            model.adminAddress = new address();

			//Return View Order
			return View("Views/Home/Order.cshtml", model);
        }

        [Route("/get-selectedCity")]
        [HttpPost]
        public async Task<IActionResult> GetSelectedCity(string selectedCity)
        {
			var model = await _sessionHelper.GetHomeModel();
			model.vNMap = new VNMap();
			if (!selectedCity.Equals("00"))
            {
                var provinces = _vnContext.Provinces.ToList();
                var districts = _vnContext.Districts.ToList();

                var districtsByProvince = (from province in provinces
                                          join district in districts
                                          on province.Code equals district.ProvinceCode
                                          where province.Code == selectedCity
                                          select new District { Code = district.Code
                                          , Name = district.Name}).ToList();
				return Json(new {districtFounds = districtsByProvince});
            }
            var nullDistrict = new List<District>();
            nullDistrict.Add(new District { Code = "000", Name = "Select a district" });

			return Json(new { districtFounds = nullDistrict });
		}

		[Route("/get-selectedDistrict")]
		[HttpPost]
        public async Task<IActionResult> GetSelectedDistrict(string selectedDistrict)
        {
			var model = await _sessionHelper.GetHomeModel();
			model.vNMap = new VNMap();
			if (!string.IsNullOrEmpty(selectedDistrict) && !selectedDistrict.Equals("000"))
			{
				var districts = _vnContext.Districts.ToList();
				var wards = _vnContext.Wards.ToList();

				var wardsByDistrict = (from district in districts
										   join ward in wards
										   on district.Code equals ward.DistrictCode
										   where district.Code == selectedDistrict
										   select new Ward
										   {
											   Code = ward.Code
										   ,
											   Name = ward.Name
										   }).ToList();
				return Json(new { wardFounds = wardsByDistrict });
			}
			var nullDistrict = new List<Ward>();
			nullDistrict.Add(new Ward { Code = "0000", Name = "Select a town" });

			return Json(new { districtFounds = nullDistrict });
		}

		[HttpPost]
        public async Task<IActionResult> HandleOrder(string c_country, string c_city, string c_district, string c_town, 
            string c_address, string c_phone, string c_order_notes, string payment_method, decimal totalPrice,
            decimal shipping)
        {
            //Create address for order
            Address orderAddress = new Address
            {
                country= c_country,
                city= c_city,
                district= c_district,
                town= c_town,
                detail = c_address,
   
			};

            //Get model from session
			var model = await _sessionHelper.GetHomeModel();

			if (model.sessionId == null)
			{
				TempData["ErrorMessage"] = "Please Login!";
				return RedirectToAction("ViewSignIn", "SignIn");
			}

			//Get cart of this user
			var userCart = model.cart;
			if (userCart == null)
			{
				return BadRequest("Invalid Cart");
			}

			var cartitems = userCart.cartitems.Where(item => item.selected == true);
			if (cartitems == null)
			{
				TempData["ErrorMessage"] = "Please Login!";
				return RedirectToAction("ViewSignIn", "SignIn");
			}

            //TotalOrder
            var totalOrder = totalPrice;

            //Get User Order
            if(userCart.userId == null)
            {
                return BadRequest("userId invalid");
			}
			var userOrder = _userRepository.GetById(userCart.userId);

            List<OrderItem> orderItemsOrder = new List<OrderItem>();
            foreach(var item in cartitems)
            {
				orderItemsOrder.Add( new OrderItem
                {
                    product = item.product,
                    color = item.color,
                    size = item.size,   
                    quantity = item.quantity,
                });
			}

            // Create status when order
            Status status = new Status();

            Order order = new Order
            {
                shipping = shipping,
                total = totalOrder,
                address = orderAddress,
                user = userOrder,
                orderitems = orderItemsOrder,
                paymentMethod = payment_method,
                orderNote = c_order_notes,
                orderDate = DateTime.Now,
                status = status.pending
			};

			if (payment_method == "payment_delivery")
            {
				List<CartItem> itemsToRemove = new List<CartItem>();
				foreach (var item in cartitems)
				{
					itemsToRemove.Add(item);

				}
				//Delete Products From Cart When Order
				foreach (var itemToRemove in itemsToRemove)
                {
                    userCart.cartitems.Remove(itemToRemove);
                }
                _cartRepository.Update(userCart, userCart.id);

                //Create Order
				_orderRepository.Create(order);

                //Send Email To Customer
                MailContent contentCustomer = new MailContent
                {
                    To = userOrder.Email,
                    Subject = "Order Confirmation",
                    Body = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n " +
                    $"   <meta charset=\"UTF-8\">\r\n   " +
                    $" <meta name=\"viewport\" content=\"width=device-width, " +
                    $"initial-scale=1.0\">\r\n   " +
                    $" <title>Thông báo: Đã nhận đơn hàng của bạn</title>\r\n " +
                    $"   <style>\r\n        body {{\r\n          " +
                    $"  font-family: Arial, sans-serif;\r\n           " +
                    $" background-color: #f4f4f4;\r\n           " +
                    $" margin: 0;\r\n            padding: 0;\r\n    " +
                    $"    }}\r\n        .container {{\r\n       " +
                    $"     max-width: 600px;\r\n            margin: 20px auto;\r\n        " +
                    $"    padding: 20px;\r\n            background-color: #fff;\r\n     " +
                    $"       border-radius: 8px;\r\n          " +
                    $"  box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n    " +
                    $"    }}\r\n        h2 {{\r\n            color: #333;\r\n   " +
                    $"     }}\r\n        p {{\r\n            margin-bottom: 20px;\r\n  " +
                    $"          line-height: 1.6;\r\n        }}\r\n        .button {{\r\n       " +
                    $"     display: inline-block;\r\n            background-color: #4CAF50;\r\n    " +
                    $"        color: white;\r\n            padding: 10px 20px;\r\n        " +
                    $"    text-align: center;\r\n            text-decoration: none;\r\n   " +
                    $"         border-radius: 4px;\r\n        }}\r\n    </style>\r\n</head>\r\n<body>\r\n  " +
                    $"  <div class=\"container\">\r\n      " +
                    $"  <h2>Thông báo: Đã nhận đơn hàng của bạn</h2>\r\n   " +
                    $"     <p>Xin chào,</p>\r\n       " +
                    $" <p>Cảm ơn bạn đã đặt hàng từ chúng tôi." +
                    $" Đơn hàng của bạn đã được nhận và đang được xử lý.</p>\r\n    " +
                    $"    <p>Chi tiết đơn hàng:</p>\r\n        <ul>\r\n    " +
                    $"        <li><strong>Mã đơn hàng:</strong> {order.id}</li>\r\n     " +
                    $"       <li><strong>Ngày đặt hàng:</strong> {order.orderDate}</li>\r\n  " +
                    $"          <li><strong>Tổng số tiền:</strong>{order.total}$</li>\r\n   " +
                    $"     </ul>\r\n      " +
                    $"  <p>Xin vui lòng kiểm tra email của bạn để theo dõi trạng thái của đơn hàng." +
                    $" Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi.</p>\r\n     " +
                    $"   <a href=\"#\" class=\"button\" style='color: white;'>Theo dõi đơn hàng</a>\r\n      " +
                    $"  <p>Cảm ơn bạn đã mua hàng!</p>\r\n    </div>\r\n</body>\r\n</html>\r\n"
				};

                //Send Email To Admin
                MailContent contentAdmin = new MailContent
                {
                    To = _mailSettings.Mail,
                    Subject = "New Order Received",
                    Body = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n  " +
                    $"  <meta charset=\"UTF-8\">\r\n   " +
                    $" <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n   " +
                    $" <title>Notification: New Order Received</title>\r\n " +
                    $"   <style>\r\n        body {{\r\n         " +
                    $"   font-family: Arial, sans-serif;\r\n         " +
                    $"   background-color: #f4f4f4;\r\n    " +
                    $"        margin: 0;\r\n           " +
                    $" padding: 0;\r\n        }}\r\n    " +
                    $"    .container {{\r\n         " +
                    $"   max-width: 600px;\r\n           " +
                    $" margin: 20px auto;\r\n        " +
                    $"    padding: 20px;\r\n           " +
                    $" background-color: #fff;\r\n       " +
                    $"" +
                    $"     border-radius: 8px;\r\n      " +
                    $"      box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n   " +
                    $"     }}\r\n        h2 {{\r\n            color: #333;\r\n " +
                    $"       }}\r\n        p {{\r\n            margin-bottom: 20px;\r\n    " +
                    $"        line-height: 1.6;\r\n        }}\r\n       " +
                    $" .button {{\r\n            display: inline-block;\r\n        " +
                    $"    background-color: #4CAF50;\r\n            color: white;\r\n  " +
                    $"          padding: 10px 20px;\r\n            text-align: center;\r\n         " +
                    $"   text-decoration: none;\r\n            border-radius: 4px;\r\n  " +
                    $"      }}\r\n    </style>\r\n</head>\r\n<body>\r\n " +
                    $"   <div class=\"container\">\r\n       " +
                    $" <h2>Notification: New Order Received</h2>\r\n  " +
                    $"      <p>Hello Admin,</p>\r\n   " +
                    $"     <p>A new order has been received. Please find the details below:</p>\r\n      " +
                    $"  <ul>\r\n            <li><strong>Order Number:</strong> {order.id}</li>\r\n  " +
                    $"          <li><strong>Order Date:</strong> {order.orderDate}</li>\r\n      " +
                    $"      <li><strong>Total Amount:</strong> ${order.total}</li>\r\n     " +
                    $"   </ul>\r\n     " +
                    $"   <p>Please take necessary actions to process the order" +
                    $" and fulfill customer's request.</p>\r\n       " +
                    $" <p>If you have any questions or concerns," +
                    $" please feel free to contact the customer.</p>\r\n   " +
                    $"     <a href=\"#\" class=\"button\">View Order Details</a>\r\n      " +
                    $"  <p>Thank you!</p>\r\n    </div>\r\n</body>\r\n</html>\r\n"
                };

				await _sendGmailService.SendMail(contentCustomer);
				await _sendGmailService.SendMail(contentAdmin);

				//Set in model
				model.cartOrder = new Cart();

				return RedirectToAction("OrderSuccess", "Order");
			}
            else if (payment_method == "payment_online")
            {
                var vnPayModel = new VnPaymentRequestModel
                {
                    Amount = totalPrice,
                    CreatedDate = DateTime.Now,
                    Description = $"{order.user.Email}-{c_phone}",
                    FullName = order.user.UserName,
                    OrderId = order.id,
                };
               
                //Create session tmp userId
                HttpContext.Session.SetString("UserID", model.sessionId);
				

				//Create Order temp if not success -> delete
				_orderRepository.Create(order);

				//Create session tmp orderId
				HttpContext.Session.SetString("OrderID", order.id);

				return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
            }
            return BadRequest("Invalid payment method!");    

        }

		[Route("/orderFail")]
		public IActionResult PaymentFail()
        {
			return View("Views/Home/OrderFail.cshtml");
		}

        [Route("/orderSuccess")]
		public IActionResult PaymentSuccess()
		{
			return View("Views/Home/OrderSuccess.cshtml");
		}

		//Payment call back
		
        public async Task<IActionResult> PaymentCallBack()
        {
			//Get order from session
			var orderId = HttpContext.Session.GetString("OrderID");
			if (orderId == null)
			{
				TempData["ErrorMessage"] = "Please Login!";
				return RedirectToAction("ViewSignIn", "SignIn");
			}
			var response = _vnPayService.PaymentExecute(Request.Query);

            if(response == null || response.VnPayResponseCode != "00")
            {
                //Delete Order in DB if failed
                _orderRepository.Delete(new Order { id = orderId });

                TempData["Message"] = $"Erron VnPay Payment: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

			//Handle when Order Success

            //Get User id from session
			var userId = HttpContext.Session.GetString("UserID");
			if (userId == null)
			{
				TempData["ErrorMessage"] = "Please Login!";
				return RedirectToAction("ViewSignIn", "SignIn");
			}

            //Get user by id
            var userOrder = _userRepository.GetById(userId);

			//Get Order by OrderId
			var order = _orderRepository.GetById(orderId);
            if(order == null)
            {
                return Content("Order Invalid");
            }

			//Get cart of this user
			var userCart = _cartRepository.GetByUserId(userId);
			if (userCart == null)
			{
				return BadRequest("Invalid Cart");
			}

			var cartitems = userCart.cartitems.Where(item => item.selected == true);
			if (cartitems == null)
			{
				TempData["ErrorMessage"] = "Please Login!";
				return RedirectToAction("ViewSignIn", "SignIn");
			}
			List<CartItem> itemsToRemove = new List<CartItem>();
			foreach (var item in cartitems)
			{
				itemsToRemove.Add(item);

			}

			//Delete Products From Cart When Order
			foreach (var itemToRemove in itemsToRemove)
			{
				userCart.cartitems.Remove(itemToRemove);
			}
			_cartRepository.Update(userCart, userCart.id);

            //Send Email To Customer
            MailContent contentCustomer = new MailContent
            {
                To = userOrder.Email,
                Subject = "Order Confirmation",
                Body = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n " +
                $"   <meta charset=\"UTF-8\">\r\n   " +
                $" <meta name=\"viewport\" content=\"width=device-width, " +
                $"initial-scale=1.0\">\r\n   " +
                $" <title>Thông báo: Đã nhận đơn hàng của bạn</title>\r\n " +
                $"   <style>\r\n        body {{\r\n          " +
                $"  font-family: Arial, sans-serif;\r\n           " +
                $" background-color: #f4f4f4;\r\n           " +
                $" margin: 0;\r\n            padding: 0;\r\n    " +
                $"    }}\r\n        .container {{\r\n       " +
                $"     max-width: 600px;\r\n            margin: 20px auto;\r\n        " +
                $"    padding: 20px;\r\n            background-color: #fff;\r\n     " +
                $"       border-radius: 8px;\r\n          " +
                $"  box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n    " +
                $"    }}\r\n        h2 {{\r\n            color: #333;\r\n   " +
                $"     }}\r\n        p {{\r\n            margin-bottom: 20px;\r\n  " +
                $"          line-height: 1.6;\r\n        }}\r\n        .button {{\r\n       " +
                $"     display: inline-block;\r\n            background-color: #4CAF50;\r\n    " +
                $"        color: white;\r\n            padding: 10px 20px;\r\n        " +
                $"    text-align: center;\r\n            text-decoration: none;\r\n   " +
                $"         border-radius: 4px;\r\n        }}\r\n    </style>\r\n</head>\r\n<body>\r\n  " +
                $"  <div class=\"container\">\r\n      " +
                $"  <h2>Thông báo: Đã nhận đơn hàng của bạn</h2>\r\n   " +
                $"     <p>Xin chào,</p>\r\n       " +
                $" <p>Cảm ơn bạn đã đặt hàng từ chúng tôi." +
                $" Đơn hàng của bạn đã được nhận và đang được xử lý.</p>\r\n    " +
                $"    <p>Chi tiết đơn hàng:</p>\r\n        <ul>\r\n    " +
                $"        <li><strong>Mã đơn hàng:</strong> {order.id}</li>\r\n     " +
                $"       <li><strong>Ngày đặt hàng:</strong> {order.orderDate}</li>\r\n  " +
                $"          <li><strong>Tổng số tiền:</strong>{order.total}$</li>\r\n   " +
                $"     </ul>\r\n      " +
                $"  <p>Xin vui lòng kiểm tra email của bạn để theo dõi trạng thái của đơn hàng." +
                $" Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi.</p>\r\n     " +
                $"   <a href=\"#\" class=\"button\" style='color: white;'>Theo dõi đơn hàng</a>\r\n      " +
                $"  <p>Cảm ơn bạn đã mua hàng!</p>\r\n    </div>\r\n</body>\r\n</html>\r\n"
			};

            //Send Email To Admin
            MailContent contentAdmin = new MailContent
            {
                To = _mailSettings.Mail,
                Subject = "New Order Received",
                Body = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n  " +
                $"  <meta charset=\"UTF-8\">\r\n   " +
                $" <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n   " +
                $" <title>Notification: New Order Received</title>\r\n " +
                $"   <style>\r\n        body {{\r\n         " +
                $"   font-family: Arial, sans-serif;\r\n         " +
                $"   background-color: #f4f4f4;\r\n    " +
                $"        margin: 0;\r\n           " +
                $" padding: 0;\r\n        }}\r\n    " +
                $"    .container {{\r\n         " +
                $"   max-width: 600px;\r\n           " +
                $" margin: 20px auto;\r\n        " +
                $"    padding: 20px;\r\n           " +
                $" background-color: #fff;\r\n       " +
                $"" +
                $"     border-radius: 8px;\r\n      " +
                $"      box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n   " +
                $"     }}\r\n        h2 {{\r\n            color: #333;\r\n " +
                $"       }}\r\n        p {{\r\n            margin-bottom: 20px;\r\n    " +
                $"        line-height: 1.6;\r\n        }}\r\n       " +
                $" .button {{\r\n            display: inline-block;\r\n        " +
                $"    background-color: #4CAF50;\r\n            color: white;\r\n  " +
                $"          padding: 10px 20px;\r\n            text-align: center;\r\n         " +
                $"   text-decoration: none;\r\n            border-radius: 4px;\r\n  " +
                $"      }}\r\n    </style>\r\n</head>\r\n<body>\r\n " +
                $"   <div class=\"container\">\r\n       " +
                $" <h2>Notification: New Order Received</h2>\r\n  " +
                $"      <p>Hello Admin,</p>\r\n   " +
                $"     <p>A new order has been received. Please find the details below:</p>\r\n      " +
                $"  <ul>\r\n            <li><strong>Order Number:</strong> {order.id}</li>\r\n  " +
                $"          <li><strong>Order Date:</strong> {order.orderDate}</li>\r\n      " +
                $"      <li><strong>Total Amount:</strong> ${order.total}</li>\r\n     " +
                $"   </ul>\r\n     " +
                $"   <p>Please take necessary actions to process the order" +
                $" and fulfill customer's request.</p>\r\n       " +
                $" <p>If you have any questions or concerns," +
                $" please feel free to contact the customer.</p>\r\n   " +
                $"     <a href=\"#\" class=\"button\">View Order Details</a>\r\n      " +
                $"  <p>Thank you!</p>\r\n    </div>\r\n</body>\r\n</html>\r\n"
            };

			await _sendGmailService.SendMail(contentCustomer);
			await _sendGmailService.SendMail(contentAdmin);

            //Delete Session tmp
            HttpContext.Session.Remove("UserID");

			TempData["Message"] = $"Payment VnPay Success!";
			return RedirectToAction("PaymentSuccess"); ;
        }

        public async Task<IActionResult> OrderSuccess()
        {
			var model = await _sessionHelper.GetHomeModel();

			if (model.sessionId == null)
			{
				TempData["ErrorMessage"] = "Please Login!";
				return RedirectToAction("ViewSignIn", "SignIn");
			}
			return View("Views/Home/OrderSuccess.cshtml", model);
        }
    }
}
