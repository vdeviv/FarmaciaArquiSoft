using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceUser.Domain
{
    public enum UserRole
    {
        Administrador,
        Cajero,
        Almacenero
    }
    public class User
    {
        #region Atributos
        public int id { get; set; }
        public string first_name { get; set; }
        public string? second_name { get; set; }
        public string last_name { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string mail { get; set; }
        public int phone { get; set; }
        public string ci { get; set; }
        public UserRole role { get; set; } = UserRole.Cajero;
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; } = DateTime.Now;
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public bool is_deleted { get; set; } = false;
        #endregion

        #region Constructor
        public User() { }

        public User(int id, string username, string password, string mail, int phone, string ci, UserRole role,
                    bool is_deleted = false, int created_by = 0, int updated_by = 0,
                    DateTime? created_at = null, DateTime? updated_at = null)
        {
            this.id = id;
            this.username = username;
            this.password = password;
            this.mail = mail;
            this.phone = phone;
            this.ci = ci;
            this.role = role;
            this.is_deleted = is_deleted;
            this.created_by = created_by;
            this.updated_by = updated_by;
            this.created_at = created_at ?? DateTime.Now;
            this.updated_at = updated_at ?? DateTime.Now;
        }
        #endregion
    }
}
