using BusinessObject.Admin;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.HomeViewModel
{
    public class HomeModel
    {
        public string? sessionId { get; set; }
        public string? sessionName {  get; set; }
        public bool? checkSessionExist {  get; set; }

        //Product
        public List<Product>? products { get; set; }
        public Product? product { get; set; }

        //Cart
        public ICollection<CartItem>? cartItems { get; set; }
        public Cart? cart { get; set; }
        public Cart? cartOrder { get; set; }

        //Order
        public VNMap? vNMap { get; set; }
        public address adminAddress { get; set; }

	}
}
