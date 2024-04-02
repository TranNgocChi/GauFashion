using BusinessObject.Models;
using DataAccess.Context;
using DataAccess.DAO.IDAO;
using DataAccess.DAO.SharedFunction;
using MongoDB.Driver;

namespace DataAccess.DAO
{
    public class CartDAO : IObjectDAO<Cart>
    {
        private readonly MongoDBContext _dbContext = new MongoDBContext();

        SharedFunctionDAO<Cart> sharedFunc = new SharedFunctionDAO<Cart>();
        public void Create(Cart cart)
        {
            try
            {
                _dbContext.Carts.InsertOne(cart);
                Console.WriteLine("Inserted Successfully!");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Delete(Cart cart)
        {
            try
            {
                var filter = Builders<Cart>.Filter.Empty;
                if(cart.userId != null)
                {
                    filter = sharedFunc.GetFilterById(c => c.userId, cart.userId);
                }
                else
                {
                    filter = sharedFunc.GetFilterById(c => c.id, cart.id);
                }
                var deleteCart = _dbContext.Carts.DeleteOne(filter);
                if (deleteCart.DeletedCount > 0)
                {
                    Console.WriteLine("Deleted Successfully!");
                }
                else
                {
                    Console.WriteLine($"Cart with id = {cart.id} or {cart.userId} can't be found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Cart GetById(string id)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(c => c.id, id);
                Cart cartFound = _dbContext.Carts.Find(filter).FirstOrDefault();
                if (cartFound != null)
                {
                    return cartFound;
                }
                else
                {
                    Console.WriteLine($"Cart with id = {id} can't be found");
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Cart GetByUserId(string userid)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(c => c.userId, userid);
                Cart cartFound = _dbContext.Carts.Find(filter).FirstOrDefault();
                if (cartFound != null)
                {
                    return cartFound;
                }
                else
                {
                    Console.WriteLine($"Cart with userid = {userid} can't be found");
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Cart> ShowAll()
        {
            List<Cart> listCart = new List<Cart>();
            try
            {
                listCart = _dbContext.Carts.Find(_ => true).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return listCart;
        }

        public void Update(Cart cart, string id)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(c => c.id, id);
                if (GetById(id) != null)
                {
                    cart.id = id;
                    _dbContext.Carts.ReplaceOne(filter, cart);
                    Console.WriteLine($"Updated Succesfully!");
                }
                else
                {
                    Console.WriteLine($"Cart with id = {id} can't be found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
