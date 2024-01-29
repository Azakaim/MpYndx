using YandexMarketService.BLL.Models;

namespace YandexMarketService.BLL.Services.Users
{
    public interface IUsersService
    {
        Task LogInAsync(UserModel user);
    }
}