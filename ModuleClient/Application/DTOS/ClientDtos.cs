using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceClient.Application.DTOS
{
    public record ClientCreateDto(
      string FirstName,
      string LastName,
      string email,
      string nit
  );

    public record ClientUpdateDto(
       string FirstName,
       string LastName,
       string email,
       string nit
   );
}
