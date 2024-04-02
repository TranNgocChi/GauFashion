using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BusinessObject.Models
{
    public class CartItem
    {
        public Product product { get; set; }
        public string color { get; set; }
        public string size { get; set; }
        public int quantity { get; set; }
        public bool selected {  get; set; }
    }

    public class Cart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }

        public decimal total { get; set; }

        public string userId { get; set; }

        public ICollection<CartItem> cartitems { get; set; }
    }
}
