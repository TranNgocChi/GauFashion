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
    public class ProductRepository : IProductRepository
    {
        ProductDAO productDAO = new ProductDAO();

        public void Create(Product obj) => productDAO.Create(obj);

        public void Delete(Product obj) => productDAO.Delete(obj);

        public void DeleteAll() => productDAO.DeleteAll();
        public Product GetById(string id) => productDAO.GetById(id);    

        public List<Product> ShowAll() => productDAO.ShowAll();
        public void Update(Product obj, string id) => productDAO.Update(obj, id);   
    }
}
