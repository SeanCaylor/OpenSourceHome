using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenSourceHome.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required(ErrorMessage = "is required")]
        [MinLength(2, ErrorMessage = "must be at least 5 characters")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "is required")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "is required.")]
        [MinLength(8, ErrorMessage = "must be at least 8 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [NotMapped]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "is required.")]
        [Compare("Password", ErrorMessage = "doesn't match password")]
        [Display(Name = "Confirm Password")]
        public string PasswordConfirm { get; set; }
        [Required(ErrorMessage = "is required")]
        [MinLength(2, ErrorMessage = "must be at least 2 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "is required")]
        [MinLength(2, ErrorMessage = "must be at least 2 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [NotMapped]
        [Display(Name = "Location")]
        public string Location { get; set; }
        public float nLongitude { get; set; }
        public float nLatitude { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /*
        Foreign Keys and Nav Properties
        */
        public int SerialNumberId { get; set; }
        public SerialNumber UserSerial { get; set; }
        public List<Post> Posts { get; set; }
        public List<UserPostLike> PostLikes { get; set; }
        public List<Reply> Replies { get; set; }

        /*
        Methods
        */
        public string FullName()
        {
            return FirstName + " " + LastName;
        }
    }
}