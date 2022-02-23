using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenSourceHome.Models
{
    public class Post
    {
        [Key] // Primary Key
        public int PostId { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "Must be more than 2 characters.")]
        [MaxLength(150, ErrorMessage = "Must be less than 150 characters.")]
        public string Topic { get; set; }

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
        public List<UserPostLike> Likes { get; set; }
        public List<Reply> Replies { get; set; }

        /*
        Methods
        */
    }
}