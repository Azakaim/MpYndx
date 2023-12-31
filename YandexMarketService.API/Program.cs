using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using YandexMarketService.BLL.Repositories;
using YandexMarketService.BLL.Services.Reviews;
using YandexMarketService.BLL.Services.Users;
using YandexMarketService.DAL.Reviews;
using YandexMarketService.DAL.Users;

var builder = WebApplication.CreateBuilder(args);

// BLL
builder.Services.AddTransient<IUsersService, UsersService>();
builder.Services.AddTransient<IReviewsService, ReviewsService>();

// DAL
builder.Services.AddScoped<IUsersRepository, PlaywrightUsersRepository>();
builder.Services.AddScoped<IReviewsRepository, PlaywrightReviewsRepository>();

builder.Services.AddControllers();
builder.Services.AddMemoryCache();



var app = builder.Build();

//app.UseExceptionHandler(e => e.Run(async context =>
//{
//    var exHandler = context.Features.GetRequiredFeature<IExceptionHandlerFeature>();
//    await context.Response.WriteAsJsonAsync(exHandler.Error);
//}));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
