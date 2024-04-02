using BusinessObject.Models;
using DataAccess.Context;
using DataAccess.DAO.IDAO;
using DataAccess.DAO.SharedFunction;
using MongoDB.Driver;

namespace DataAccess.DAO
{
    public class ProductDAO : IObjectDAO<Product>
    {
        private readonly MongoDBContext _dbContext = new MongoDBContext();

        SharedFunctionDAO<Product> sharedFunc = new SharedFunctionDAO<Product>();
        public void Create(Product product)
        {
            try
            {
                _dbContext.Products.InsertOne(product);
                Console.WriteLine("Inserted Successfully!");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Delete(Product product)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(p => p.id, product.id);
                var deleteProduct = _dbContext.Products.DeleteOne(filter);
                if (deleteProduct.DeletedCount > 0)
                {
                    Console.WriteLine("Deleted Successfully!");
                }
                else
                {
                    Console.WriteLine($"Product with id = {product.id} can't be found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteAll()
        {
            try
            {
                var deleteAllProduct = _dbContext.Products.DeleteMany(_ => true);
                Console.WriteLine("Deleted All Successfully!");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public Product GetById(string id)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(p => p.id, id);
                Product productFound = _dbContext.Products.Find(filter).FirstOrDefault();
                if (productFound != null)
                {
                    return productFound;
                }
                else
                {
                    Console.WriteLine($"Product with id = {id} can't be found");
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Product> ShowAll()
        {
            List<Product> listProduct = new List<Product>();
            try
            {
                listProduct = _dbContext.Products.Find(_ => true).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return listProduct;
        }

        public void Update(Product product, string id)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(p => p.id, id);
                if (GetById(id) != null)
                {
                    product.id = id;
                    _dbContext.Products.ReplaceOne(filter, product);
                    Console.WriteLine($"Updated Succesfully!");
                }
                else
                {
                    Console.WriteLine($"Product with id = {id} can't be found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
