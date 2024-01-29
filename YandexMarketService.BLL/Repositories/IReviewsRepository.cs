namespace YandexMarketService.BLL.Repositories
{
    public interface IReviewsRepository
    {
        Task HandleReviewsAsync(string userId);
    }
}