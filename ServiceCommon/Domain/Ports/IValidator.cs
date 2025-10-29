using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;

namespace ServiceCommon.Domain.Ports
{
    public interface IValidator<T>
    {
        Result Validate(T entity);
    }
}
