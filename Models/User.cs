using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Models
{
    public class User
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }


        [Required]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
