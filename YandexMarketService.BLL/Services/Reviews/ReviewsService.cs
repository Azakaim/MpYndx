using YandexMarketService.BLL.Repositories;

namespace YandexMarketService.BLL.Services.Reviews
{
    public class ReviewsService : IReviewsService
    {
        readonly IReviewsRepository _reviewsRepository;

        public ReviewsService(IReviewsRepository reviewsRepository) => _reviewsRepository = reviewsRepository;

        public async Task HandleReviewsAsync()
        {
            await _reviewsRepository.HandleReviewsAsync("anonogo");
        }
    }
}
