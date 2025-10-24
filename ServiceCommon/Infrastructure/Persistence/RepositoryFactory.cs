using ServiceCommon.Domain.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommon.Infrastructure.Persistence
{
    public abstract class RepositoryFactory
    {
        public abstract IRepository<T> CreateRepository<T>() where T : class;

    }
}
