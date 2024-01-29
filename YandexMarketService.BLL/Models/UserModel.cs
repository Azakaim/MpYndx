namespace YandexMarketService.BLL.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
