namespace BosBelme.ViewModels
{
    public class EnterNameViewModel
    {
        public string? RoomCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите ваше имя")]
        public string PlayerName { get; set; } = string.Empty;
    }
}
