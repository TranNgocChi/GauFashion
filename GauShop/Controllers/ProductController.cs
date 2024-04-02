using BusinessObject.HomeViewModel;
using BusinessObject.Models;
using DataAccess.Repository.IObjectRepository;
using DataAccess.Repository.ObjectRepository;
using GauShop.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace GauShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository productRepository = new ProductRepository();
        private readonly SessionHelper _sessionHelper;

        public ProductController(SessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

        [Route("/products")]
        public async Task<IActionResult> ViewProducts()
        {
            List<Product> allProducts = productRepository.ShowAll();
            if(allProducts.Count == 0)
            {
                //
            }
            var model = await _sessionHelper.GetHomeModel();
            model.products  = allProducts;
            return View("Views/Home/Products.cshtml", model);
            
        }

        [Route("/products/{id}")]
        public async Task<IActionResult> ViewProductDetail(string id)
        {
            Product foundProduct = productRepository.GetById(id);
            if(foundProduct == null)
            {
                //
            }
            var model = await _sessionHelper.GetHomeModel();
            model.product = foundProduct;
            return View("Views/Home/ProductDetail.cshtml", model);
        }
    }
}
