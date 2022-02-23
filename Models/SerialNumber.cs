using System;
using System.ComponentModel.DataAnnotations;

namespace OpenSourceHome.Models
{
    public class SerialNumber
    {
        [Key]
        public int SerialNumberId { get; set; }
        public string HashedSN { get; set; }
        public int Downloads { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /*
        Foreign Keys and Nav Properties
        */

        /*
        Methods
        */
    }
}