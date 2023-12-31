namespace YandexMarketService.BLL.Repositories
{
    public interface IUsersRepository
    {
        Task LogInAsync(int userId);
    }
}