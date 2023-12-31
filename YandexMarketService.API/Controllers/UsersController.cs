using Microsoft.AspNetCore.Mvc;
using YandexMarketService.BLL.Services.Users;

namespace YandexMarketService.API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class UsersController : ControllerBase
    {
        readonly IUsersService _usersService;

        public UsersController(IUsersService usersService) => _usersService = usersService;

        [HttpGet("users/login")]
        public async Task<IActionResult> LogInAsync()
        {
            await _usersService.LogInAsync();

            return Ok();
        }
    }
}
