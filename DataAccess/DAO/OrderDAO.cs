using BusinessObject.Models;
using DataAccess.Context;
using DataAccess.DAO.IDAO;
using DataAccess.DAO.SharedFunction;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class OrderDAO : IObjectDAO<Order>
    {
        private readonly MongoDBContext _dbContext = new MongoDBContext();

        SharedFunctionDAO<Order> sharedFunc = new SharedFunctionDAO<Order>();
        public void Create(Order order)
        {
            try
            {
                _dbContext.Orders.InsertOne(order);
                Console.WriteLine("Inserted Successfully!");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Delete(Order order)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(o => o.id, order.id);
                var deleteOrder = _dbContext.Orders.DeleteOne(filter);
                if (deleteOrder.DeletedCount > 0)
                {
                    Console.WriteLine("Deleted Successfully!");
                }
                else
                {
                    Console.WriteLine($"Order with id = {order.id} can't be found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Order GetById(string id)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(o => o.id, id);
                Order orderFound = _dbContext.Orders.Find(filter).FirstOrDefault();
                if (orderFound != null)
                {
                    return orderFound;
                }
                else
                {
                    Console.WriteLine($"Order with id = {id} can't be found");
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

		public Order GetByUserId(string userId)
		{
			try
			{
				var filter = sharedFunc.GetFilterById(o => o.user.Id, userId);
				Order orderFound = _dbContext.Orders.Find(filter).FirstOrDefault();
				if (orderFound != null)
				{
					return orderFound;
				}
				else
				{
					Console.WriteLine($"Order with userid = {userId} can't be found");
					return null;
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}

		public List<Order> ShowAll()
        {
            List<Order> listOrder = new List<Order>();
            try
            {
                listOrder = _dbContext.Orders.Find(_ => true).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return listOrder;
        }

        public void Update(Order order, string id)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(o => o.id, id);
                if (GetById(id) != null)
                {
                    order.id = id;
                    _dbContext.Orders.ReplaceOne(filter, order);
                    Console.WriteLine($"Updated Succesfully!");
                }
                else
                {
                    Console.WriteLine($"Order with id = {id} can't be found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
