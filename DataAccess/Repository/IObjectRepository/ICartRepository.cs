using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IObjectRepository
{
    public interface ICartRepository
    {
        public List<Cart> ShowAll();
        public void Create(Cart obj);
        public void Update(Cart obj, string id);
        public void Delete(Cart obj);
        public Cart GetById(string id);
        public Cart GetByUserId(string id);
    }
}
