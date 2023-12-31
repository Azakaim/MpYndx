namespace YandexMarketService.BLL.Repositories
{
    public interface IReviewsRepository
    {
        Task HandleReviewsAsync(int userId);
    }
}