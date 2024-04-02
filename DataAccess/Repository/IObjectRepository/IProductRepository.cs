using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IObjectRepository
{
    public interface IProductRepository
    {
        public List<Product> ShowAll();
        public void Create(Product obj);
        public void Update(Product obj, string id);
        public void Delete(Product obj);
        public Product GetById(string id);
        public void DeleteAll();
    }
}
