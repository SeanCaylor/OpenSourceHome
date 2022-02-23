using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenSourceHome.Models
{
    [NotMapped]
    public class Keymaster
    {
        [Required(ErrorMessage = "is required.")]
        [Display(Name = "UserSNInput")]
        public string UserSNInput { get; set; }
    }
}