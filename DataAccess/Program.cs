using BusinessObject;
using BusinessObject.Models;
using DataAccess.DAO;
using DataAccess.Repository.ObjectRepository;
using System.Runtime.InteropServices;

class Program
{
    static void AddProduct()
    {
        ProductDAO productDAO = new ProductDAO();
        productDAO.Create(new Product("Tree pot", "Original package design from house", "Over three years in business, We’ve had the chance to work on a variety of projects, with companies", 1, 25, "Trending", "images/product/evan-mcdougall-qnh1odlqOmk-unsplash.jpeg"));
        productDAO.Create(new Product("Juice Drinks", "Nature made another world", "Over three years in business, We’ve had the chance to work on a variety of projects, with companies", 1, 45, "New Arrival", "images/product/jordan-nix-CkCUvwMXAac-unsplash.jpeg"));
        productDAO.Create(new Product("Package", "Original package design from house", "Over three years in business, We’ve had the chance to work on a variety of projects, with companies", 1, 50, "Top Trend", "images/product/nature-zen-3Dn1BZZv3m8-unsplash.jpeg"));
        productDAO.Create(new Product("Fashion set", "Costume package", "Over three years in business, We’ve had the chance to work on a variety of projects, with companies", 1, 35, "Discounted Price", "images/product/team-fredi-8HRKoay8VJE-unsplash.jpeg"));
        productDAO.Create(new Product("Medicine", "Package design", "Over three years in business, We’ve had the chance to work on a variety of projects, with companies", 1, 100, "Best Sold", "images/product/quokkabottles-kFc1_G1GvKA-unsplash.jpeg"));
        productDAO.Create(new Product("Bottle", "Original design from house", "Over three years in business, We’ve had the chance to work on a variety of projects, with companies", 1, 200, "New", "images/product/anis-m-WnVrO-DvxcE-unsplash.jpeg"));
    }
    static void AddUser()
    {
        UserDAO userDAO = new UserDAO();
        userDAO.Create(new User { UserName = "MeoMeo", Email = "Meo@gmail.com", PasswordHash = "123456" });
        userDAO.Create(new User { UserName = "GauGau", Email = "Gau@gmail.com", PasswordHash = "123456" });
    }
    static void Main(string[] args)
    {
        CartDAO cartDAO = new CartDAO();
        cartDAO.Delete(new Cart
        {
            userId = "f6cbf9fb-d022-4502-901d-08a96dd6cd0e"
        });
    }
}