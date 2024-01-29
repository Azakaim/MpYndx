using YandexMarketService.BLL.Models;

namespace YandexMarketService.BLL.Repositories
{
    public interface IUsersRepository
    {
        Task LogInAsync(UserModel user);
    }
}