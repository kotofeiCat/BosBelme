namespace BosBelme.ViewModels
{
    public class JoinRoomViewModel
    {
        [Required(ErrorMessage = "Введите код комнаты")]
        public string RoomCode { get; set; } = string.Empty;
    }
}
