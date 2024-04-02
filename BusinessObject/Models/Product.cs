using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        [StringLength(50)]
        public string type { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Ban da nhap qua so ki tu cho phep")]
        public string description { get; set; }
        public string color { get; set; }
        public string size { get; set; }
        public int quantity { get; set; }

        [Required]
        public decimal price { get; set; }

        [StringLength(50)]
        public string featured { get; set; }

        [Required]
        public string image { get; set; }

        public Product() { }
        public Product(string Name, string Type, string Description, int Quantity, decimal Price, string Featured, string Image)
        {
            name = Name;
            type = Type;
            description = Description;
            quantity = Quantity;
            price = Price;
            featured = Featured;
            image = Image;
        }

        public void ShowInfo()
        {
            Console.WriteLine($"Id: {id}\nName: {name}\nType: {type}\nDescription: {description}\nColor: {color}\nSize: {size}\nQuantity: {quantity}\nPrice:{price}\nFeatured:{featured}\nImage: {image}");
            Console.WriteLine("----------------------------");
        }

    }
}
