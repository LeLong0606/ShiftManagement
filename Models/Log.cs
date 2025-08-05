using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class Log
    {
        [Key]
        public int LogID { get; set; }

        [ForeignKey("Users")]
        public int? UserID { get; set; }
        public User User { get; set; }

        public string Action { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
