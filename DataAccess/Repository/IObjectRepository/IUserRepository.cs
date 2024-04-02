using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IObjectRepository
{
    public interface IUserRepository
    {
        public List<User> ShowAll();
        public void Create(User obj);
        public void Update(User obj, string id);
        public void Delete(User obj);
        public User GetById(string id);
        public User GetByEmail(string email);
        public void DeleteAll();
        public bool IsEmailExists(string email);
        public bool AuthenticateUser(string email, string password);
    }
}
