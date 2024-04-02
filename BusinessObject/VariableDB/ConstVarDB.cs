using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.ConstVariable
{
    public class ConstVarDB
    {
        public const string connectionString = "mongodb://localhost:27017/";
        public const string databaseName = "GauFashion";

        public const string UserCollection = "User";
        public const string ProductCollection = "Product";
        public const string CartCollection = "Cart";
        public const string OrderCollection = "Order";
        
    }
}
