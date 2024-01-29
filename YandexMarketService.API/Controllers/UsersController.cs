using Microsoft.AspNetCore.Mvc;
using YandexMarketService.BLL.Models;
using YandexMarketService.BLL.Services.Users;

namespace YandexMarketService.API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class UsersController : ControllerBase
    {
        readonly IUsersService _usersService;

        public UsersController(IUsersService usersService) => _usersService = usersService;

        [HttpPost("users/login")]
        public async Task<IActionResult> LogInAsync(UserModel user)
        {
            await _usersService.LogInAsync(user);

            return Ok();
        }
    }
}
