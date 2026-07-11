namespace BosBelme.ViewModels
{
    public class CreateRoomViewModel
    {
        [Required(ErrorMessage = "Введите ваше имя")]
        [StringLength(20, ErrorMessage = "Имя слишком длинное")]
        public string PlayerName { get; set; } = string.Empty;
    }
}
