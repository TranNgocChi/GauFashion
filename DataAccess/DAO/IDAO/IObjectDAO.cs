using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO.IDAO
{
    public interface IObjectDAO<T>
    {
        public List<T> ShowAll();
        public void Create(T obj);
        public void Update(T obj, string id);
        public void Delete(T obj);
        public T GetById(string id);
        public void DeleteAll() { }

    }
}
