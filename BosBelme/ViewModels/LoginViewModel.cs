namespace BosBelme.ViewModels
{
    // Модель представления для входа пользователя
    public class LoginViewModel
    {
        [MaxLength(50)]
        [Required]
        public string NameOrEmail { get; set; } = string.Empty;

        [MaxLength(100)]
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}