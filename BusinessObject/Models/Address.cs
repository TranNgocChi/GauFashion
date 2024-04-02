using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Address
    {
        public string country { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string town { get; set; }
        public string detail { get; set; }
		public string phone { get; set; }
	}
}
