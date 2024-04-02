using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{

    public class Status
    {
        public string pending = "Pending";
		public string processing = "Processing";
		public string shipped = "Shipped";
		public string delivered = "Delivered";
		public string cancelled = "Cancelled";
		public string completed = "Completed";
	}

    public class OrderItem
    {
        public Product product { get; set; }
        public string color { get; set; }
        public string size { get; set; }
        public int quantity { get; set; }

    }
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public decimal shipping { get; set; }
        public decimal total { get; set; }
        public Address address { get; set; }    
        public User user { get; set; }
        public ICollection<OrderItem> orderitems { get; set; }
        public string paymentMethod { get; set; }
        public string? orderNote {  get; set; }
        public DateTime orderDate { get; set; }
        public string status {  get; set; }
    }
}
