using ServiceCommon.Domain.Ports;
using ServiceCommon.Infrastructure.Persistence;
using ServiceProvider.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProvider.Infraestructure
{
    public sealed class ProviderRepositoryFactory : RepositoryFactory
    {
        public override IRepository<T> CreateRepository<T>() where T : class
        {
            if (typeof(T) == typeof(Provider))
                return (IRepository<T>)new ProviderRepository();

            throw new NotImplementedException($"No existe implementación CRUD para {typeof(T).Name}");
        }
    }
}
