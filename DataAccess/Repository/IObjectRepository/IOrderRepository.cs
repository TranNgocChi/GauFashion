using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IObjectRepository
{
    public interface IOrderRepository
    {
        public List<Order> ShowAll();
        public void Create(Order obj);
        public void Update(Order obj, string id);
        public void Delete(Order obj);
        public Order GetById(string id);
        public Order GetByUserId(string userId);
    }
}
