namespace BosBelme.ViewModels
{
    public class JoinRoomCodeViewModel
    {
        [Required(ErrorMessage = "Введите код комнаты")]
        public string RoomCode { get; set; } = string.Empty;
    }
}
