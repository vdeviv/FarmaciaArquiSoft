using ServiceUser.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceCommon.Infrastructure.Persistence;
using ServiceCommon.Domain.Ports;

namespace ServiceUser.Infraestructure.Persistence
{
    public sealed class UserRepositoryFactory : RepositoryFactory
    {
        public override IRepository<T> CreateRepository<T>() where T : class
        {
            if (typeof(T) == typeof(User))
                return (IRepository<T>)new UserRepository();

            throw new NotImplementedException(
                $"No existe implementación CRUD para {typeof(T).Name}"
            );
        }
    }
}
