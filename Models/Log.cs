using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class Log
    {
        [Key]
        public int LogID { get; set; }
        public int? UserID { get; set; }
        public string? Action { get; set; }
        public string? Description { get; set; }
        public DateTime Timestamp { get; set; }

        public User? User { get; set; }
    }
}
