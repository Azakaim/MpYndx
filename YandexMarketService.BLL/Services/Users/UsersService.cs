using YandexMarketService.BLL.Models;
using YandexMarketService.BLL.Repositories;

namespace YandexMarketService.BLL.Services.Users
{
    public class UsersService : IUsersService
    {
        readonly IUsersRepository _usersRepository;

        public UsersService(IUsersRepository usersRepository) => _usersRepository = usersRepository;

        public async Task LogInAsync(UserModel user)
        {
            //try
            //{
            user.Id = 1;
            await _usersRepository.LogInAsync(user);
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }
    }
}
