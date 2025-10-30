using System;

namespace ServiceProvider.Domain
{
    public class Provider
    {
        public int id { get; set; }

        public string first_name { get; set; } = string.Empty;
        public string? second_name { get; set; }
        public string last_first_name { get; set; } = string.Empty;
        public string? last_second_name { get; set; }

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

        public string FullName =>
            $"{first_name} {second_name} {last_first_name} {last_second_name}".Replace("  ", " ").Trim();
    }
}
