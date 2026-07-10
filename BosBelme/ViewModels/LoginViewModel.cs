using System.ComponentModel.DataAnnotations;

namespace BosBelme.ViewModels
{
    public class LoginViewModel
    {
        [MaxLength(50)]
        [Required]
        public string NameOrEmail { get; set; }

        [MaxLength(100)]
        [Required]
        public string Password { get; set; }
    }
}