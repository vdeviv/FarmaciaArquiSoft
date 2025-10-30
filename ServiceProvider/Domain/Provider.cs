using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProvider.Domain
{
    public class Provider
    {
        public int id { get; set; }
        public string first_name { get; set; } = string.Empty;
        public string last_name { get; set; } = string.Empty;
        public string? nit { get; set; }
        public string? address { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public bool is_deleted { get; set; } = false;
        public bool status { get; set; } = true; 

        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; } = DateTime.Now;
        public int? created_by { get; set; }
        public int? updated_by { get; set; }

        public Provider() { }

        public Provider(
            int id,
            string first_name,
            string last_name,
            string? nit,
            string? address,
            string? email,
            string? phone,
            bool is_deleted = false,
            bool status = true,
            int? created_by = null,
            int? updated_by = null,
            DateTime? created_at = null,
            DateTime? updated_at = null)
        {
            this.id = id;
            this.first_name = first_name;
            this.last_name = last_name;
            this.nit = nit;
            this.address = address;
            this.email = email;
            this.phone = phone;
            this.is_deleted = is_deleted;
            this.status = status;
            this.created_by = created_by;
            this.updated_by = updated_by;
            this.created_at = created_at ?? DateTime.Now;
            this.updated_at = updated_at ?? DateTime.Now;
        }
    }
}

