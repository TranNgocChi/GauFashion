using BusinessObject.Models;
using DataAccess.ConstVariable;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Context
{
    public class MongoDBContext : IdentityDbContext<User>
    {
        public MongoDBContext(DbContextOptions<MongoDBContext> options) : base(options)
        {
        }
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public IMongoCollection<User> Users { get; set; }
        public IMongoCollection<Product> Products { get; set; }
        public IMongoCollection<Cart> Carts { get; set; }
        public IMongoCollection<Order> Orders { get; set; }
        public MongoDBContext()
        {
            _client = new MongoClient(ConstVarDB.connectionString);
            _database = _client.GetDatabase(ConstVarDB.databaseName);

            this.Users = _database.GetCollection<User>(ConstVarDB.UserCollection);
            this.Products = _database.GetCollection<Product>(ConstVarDB.ProductCollection);
            this.Carts = _database.GetCollection<Cart>(ConstVarDB.CartCollection);
            this.Orders = _database.GetCollection<Order>(ConstVarDB.OrderCollection);
        }

    }
}
