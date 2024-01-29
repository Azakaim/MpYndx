using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using YandexMarketService.BLL.Hubs;
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
builder.Services.AddSignalR();

builder.Services.AddCors(setup => setup.AddDefaultPolicy(conf => 
{
    conf.WithOrigins("http://localhost:8080").AllowAnyHeader().WithMethods("GET", "POST").AllowCredentials();
}));


var app = builder.Build();

//app.UseExceptionHandler(e => e.Run(async context =>
//{
//    var exHandler = context.Features.GetRequiredFeature<IExceptionHandlerFeature>();
//    await context.Response.WriteAsJsonAsync(exHandler.Error);
//}));

// TODO: add real cors policy
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors();
app.MapControllers();
app.MapHub<LogsHub>("/api/v1/logs-hub");

app.Run();
