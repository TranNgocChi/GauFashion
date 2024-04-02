using BusinessObject.Models;
using DataAccess.Context;
using DataAccess.DAO.IDAO;
using DataAccess.DAO.SharedFunction;
using DataAccess.PasswordHash;
using MongoDB.Driver;

namespace DataAccess.DAO
{
    public class UserDAO : IObjectDAO<User>
    {
        private readonly MongoDBContext _dbContext = new MongoDBContext();
        private readonly IPasswordHasherService _passwordHasher = new PasswordHasherService();

        SharedFunctionDAO<User> sharedFunc = new SharedFunctionDAO<User>();
        public void Create(User user)
        {
            try
            {
                user.createdDate = DateTime.Now;
                user.updatedDate = DateTime.Now;
                _dbContext.Users.InsertOne(user);
                Console.WriteLine("Inserted Successfully!");
                
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Delete(User user)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(u => u.Id, user.Id);
                var deleteUser = _dbContext.Users.DeleteOne(filter);    
                if(deleteUser.DeletedCount > 0)
                {
                    Console.WriteLine("Deleted Successfully!");
                }
                else
                {
                    Console.WriteLine($"User with id = {user.Id} can't be found");
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
                var deleteAllUser = _dbContext.Users.DeleteMany(_ => true);
                Console.WriteLine("Deleted All Successfully!");
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        public User? GetById(string id)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(u => u.Id, id);
                User userFound = _dbContext.Users.Find(filter).FirstOrDefault();
                if(userFound != null)
                {
                    return userFound;
                }
                else
                {
                    Console.WriteLine($"User with id = {id} can't be found");
                    return null;
                }
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public User? GetByEmail(string email)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(u => u.Email, email);
                User userFound = _dbContext.Users.Find(filter).FirstOrDefault();
                if (userFound != null)
                {
                    return userFound;
                }
                else
                {
                    Console.WriteLine($"User with email = {email} can't be found");
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<User> ShowAll()
        {
            List<User> listUser = new List<User>();
            try
            {
                listUser = _dbContext.Users.Find(_ => true).ToList();
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return listUser;
        }

        public void Update(User user, string id)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(u => u.Id, id);
                if (GetById(id) != null)
                {
                    user.Id = id;
                    user.createdDate = GetById(id).createdDate;
                    user.updatedDate = DateTime.Now;
                    _dbContext.Users.ReplaceOne(filter, user);
                    Console.WriteLine($"Updated Succesfully!");
                }
                else
                {
                    Console.WriteLine($"User with id = {id} can't be found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool IsEmailExists(string email)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(u => u.Email, email);
                var user = _dbContext.Users.Find(filter).FirstOrDefault();
                return user != null;
            }
            catch( Exception ex )
            {
                throw new Exception(ex.Message);
            }
        }

        public bool AuthenticateUser(string email, string password)
        {
            try
            {
                var filter = sharedFunc.GetFilterById(u => u.Email, email);
                var user = _dbContext.Users.Find(filter).FirstOrDefault();
                if(user != null)
                {
                    var result = _passwordHasher.VerifyPassword(password, user.PasswordHash);
                    return result;
                }
                return false;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
