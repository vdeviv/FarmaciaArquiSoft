using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommon.Domain.Ports
{
    public interface IEncryptionService
    {
        string EncryptId(int id);
        int DecryptId(string encryptedId);
    }
}
