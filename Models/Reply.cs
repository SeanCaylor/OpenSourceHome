using System;
using System.ComponentModel.DataAnnotations;

namespace OpenSourceHome.Models
{
    public class Reply
    {
        [Key] // Primary Key
        public int ReplyId { get; set; }
        [Required]
        [MinLength(2, ErrorMessage = "Must be more than 2 characters.")]
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /*
        Foreign Keys and Nav Properties
        */
        public int UserId { get; set; }
        public User Author { get; set; }
        public int PostId { get; set; }
        public Post Thread { get; set; }

        /*
        Methods
        */
    }
}