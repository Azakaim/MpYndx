using Microsoft.AspNetCore.Mvc;
using YandexMarketService.BLL.Services.Reviews;

namespace YandexMarketService.API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class ReviewsController : ControllerBase
    {
        readonly IReviewsService _reviewsService;

        public ReviewsController(IReviewsService reviewsService) => _reviewsService = reviewsService;

        [HttpGet("reviews/handling")]
        public async Task<IActionResult> HandleReviews()
        {
            await _reviewsService.HandleReviewsAsync();

            return Ok();
        }
    }
}