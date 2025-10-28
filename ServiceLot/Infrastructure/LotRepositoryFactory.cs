using ServiceLot.Domain;
using ServiceCommon.Domain.Ports;
using ServiceCommon.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ServiceLot.Infrastructure
{
    public class LotRepositoryFactory : RepositoryFactory
    {
        public override IRepository<T> CreateRepository<T>() where T : class
        {
            if (typeof(T) == typeof(Lot))
                return (IRepository<T>)new LotRepository();

            throw new NotImplementedException($"No existe implementación CRUD para {typeof(T).Name}");
        }
    }
}
