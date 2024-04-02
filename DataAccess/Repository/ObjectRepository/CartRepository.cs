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
    public class CartRepository : ICartRepository
    {
        CartDAO cartDAO = new CartDAO();
        public void Create(Cart obj) => cartDAO.Create(obj);  
        public void Delete(Cart obj) => cartDAO.Delete(obj);
        public Cart GetById(string id) => cartDAO.GetById(id);
        public Cart GetByUserId(string id) => cartDAO.GetByUserId(id);
        public List<Cart> ShowAll() => cartDAO.ShowAll();
        public void Update(Cart obj, string id) => cartDAO.Update(obj, id); 
    }
}
