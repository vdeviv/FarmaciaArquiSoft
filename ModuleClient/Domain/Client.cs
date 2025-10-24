using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceClient.Domain
{
    public class Client
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string nit { get; set; }
        public string email { get; set; }
        public bool is_deleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; } = DateTime.Now;
        public int? created_by { get; set; }
        public int? updated_by { get; set; }

        public Client()
        {

        }

        public Client(int id, string first_name, string last_name, string nit, string email, bool is_deleted = false, int created_by = 0, int updated_by = 0,
                    DateTime? created_at = null, DateTime? updated_at = null)
        {
            this.id = id;
            this.first_name = first_name;
            this.last_name = last_name;
            this.nit = nit;
            this.email = email;
            this.is_deleted = is_deleted;
            this.created_by = created_by;
            this.updated_by = updated_by;
            this.created_at = created_at ?? DateTime.Now;
            this.updated_at = updated_at ?? DateTime.Now;
        }
    }
}
