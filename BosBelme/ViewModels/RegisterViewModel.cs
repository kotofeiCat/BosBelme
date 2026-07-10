using System.ComponentModel.DataAnnotations;

namespace BosBelme.ViewModels
{
    public class RegisterViewModel
    {   
        [MaxLength(20)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        [Required]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        [Compare("Password")]
        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}