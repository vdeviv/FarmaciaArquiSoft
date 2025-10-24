using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommon.Domain.Ports
{
    public interface IRepository<T> where T : class
    {
        Task<T> Create(T entity);
        Task<T?> GetById(T entity);
        Task<IEnumerable<T>> GetAll();
        Task Update(T entity);
        Task Delete(T entity);
    }
}
