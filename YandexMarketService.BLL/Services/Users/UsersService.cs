using YandexMarketService.BLL.Repositories;

namespace YandexMarketService.BLL.Services.Users
{
    public class UsersService : IUsersService
    {
        readonly IUsersRepository _usersRepository;

        public UsersService(IUsersRepository usersRepository) => _usersRepository = usersRepository;

        public async Task LogInAsync()
        {
            //try
            //{
                await _usersRepository.LogInAsync(1);
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }
    }
}
