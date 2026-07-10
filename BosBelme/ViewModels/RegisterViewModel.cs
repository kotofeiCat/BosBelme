using System.ComponentModel.DataAnnotations;

namespace BosBelme.ViewModels
{
    public class RegisterViewModel
    {   
        [MaxLength(20)]
        [Required]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(100)]
        [Required]
        public string Password { get; set; }

        [MaxLength(100)]
        [Compare("Password")]
        [Required]
        public string ConfirmPassword { get; set; }
    }
}