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
    public class UserRepository : IUserRepository
    {
        UserDAO userDAO = new UserDAO();
        public void Create(User user) => userDAO.Create(user);

        public void Delete(User user) => userDAO.Delete(user);

        public void DeleteAll() => userDAO.DeleteAll();

        public User GetById(string id) => userDAO.GetById(id);

        public User GetByEmail(string email) => userDAO.GetByEmail(email);

        public List<User> ShowAll() => userDAO.ShowAll();

        public void Update(User user, string id) => userDAO.Update(user, id);
        public bool IsEmailExists(string email) => userDAO.IsEmailExists(email);

        public bool AuthenticateUser(string email, string password) => userDAO.AuthenticateUser(email, password);
    }
}
