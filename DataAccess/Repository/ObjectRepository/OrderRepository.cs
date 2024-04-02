using BusinessObject.Models;
using DataAccess.DAO;
using DataAccess.Repository.IObjectRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.ObjectRepository
{
    public class OrderRepository : IOrderRepository
    {
        OrderDAO orderDAO = new OrderDAO();
        public void Create(Order obj) => orderDAO.Create(obj);
        public void Delete(Order obj) => orderDAO.Delete(obj);
        public Order GetById(string id) => orderDAO.GetById(id);
		public Order GetByUserId(string userId) => orderDAO.GetByUserId(userId);
		public List<Order> ShowAll() => orderDAO.ShowAll();
        public void Update(Order obj, string id) => orderDAO.Update(obj, id);

    }
}
