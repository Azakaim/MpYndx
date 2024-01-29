using Microsoft.AspNetCore.Mvc;
using YandexMarketService.BLL.Models;
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
        public async Task<IActionResult> HandleReviewsAsync()
        {
            await _reviewsService.HandleReviewsAsync();

            return Ok();
        }

        [HttpPost("reviews/template")]
        public async Task<IActionResult> UploadReviewesTemplateAsync([FromForm] ReviewesTemplateModel templateModel)
        {
            var file = templateModel.ReviewsTemplateFile.FirstOrDefault();
            using FileStream fs = new(file.FileName, FileMode.Create);
            await file.CopyToAsync(fs);

            return Ok();
        }
    }
}