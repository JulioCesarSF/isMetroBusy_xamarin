using System;
using System.Collections.Generic;
using System.Text;

namespace isMetroBusy.DAL.DAO
{
    public interface IGenericDAO<T> where T : class
    {
        T Add(T entity, bool checkExists = true);
        bool Update(T entity);
        List<T> GetAll();
        bool Delete(T entity);
    }
}
