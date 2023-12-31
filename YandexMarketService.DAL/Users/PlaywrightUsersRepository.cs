using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using YandexMarketService.BLL.Repositories;

namespace YandexMarketService.DAL.Users
{
    public class PlaywrightUsersRepository : IUsersRepository
    {
        readonly IConfiguration _configuration;
        readonly IMemoryCache _memoryCache;

        public PlaywrightUsersRepository(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _memoryCache = memoryCache;
        }

        public async Task LogInAsync(int userId)
        {
            string? email = _configuration["YANDEX_MARKET_USER_EMAIL"];
            string? password = _configuration["YANDEX_MARKET_USER_PASSWORD"];

            //Create instance plwr
            using var playwright = await Playwright.CreateAsync();
            await Console.Out.WriteLineAsync($"\nplaywright was created. {DateTime.Now}\n");
            var browser = await playwright.Firefox.LaunchAsync(
            new BrowserTypeLaunchOptions()
            {
                //Proxy = new Proxy
                //{
                //    Username = "EMfZb7",
                //    Password = "M2nmRt",
                //    Server = "185.200.170.3:9122"
                //},
                Headless = false,//if false will include visable browser
                SlowMo = new Random().Next(1000, 1500),
                //ExecutablePath = "/root/.npm/_npx/e41f203b7505f1fb/node_modules/playwright-core/lib/server/firefox"


            });
            await Console.Out.WriteLineAsync($"\nthe browser was opened. {DateTime.Now}\n");

            //create context
            var context = await browser.NewContextAsync(new BrowserNewContextOptions()
            {
                //work with HAR DATA
                RecordHarContent = HarContentPolicy.Embed,
                RecordHarMode = HarMode.Full,
                RecordHarPath = Path.Combine("HarData", "HAR_FILE"),//write in a file
            });

            await Console.Out.WriteLineAsync($"\nthe context was created. {DateTime.Now}\n");

            //timeout of the wait
            //context.SetDefaultTimeout(60000);
            //context.SetDefaultNavigationTimeout(60000);
            context.SetDefaultTimeout(10_000);
            context.SetDefaultNavigationTimeout(10_000);

            IPage page;

            #region Read Coockie 
            //List<Cookie> cookiesForAdd = new List<Cookie>();
            if (File.Exists("context.json"))
            {
                context = await browser.NewContextAsync(new()
                {
                    StorageStatePath = "context.json"
                });

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                page = await _memoryCache.GetOrCreateAsync(userId, async x =>
                    {
                        x.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);   // TODO: set real expiration time
                        return await context.NewPageAsync();
                    });
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                await page.GotoAsync(@"https://partner.market.yandex.ru/business", new PageGotoOptions { Timeout = 60000 });
                //await page.WaitForDownloadAsync(new PageWaitForDownloadOptions { Timeout = 60000 }) ;// WaitForTimeoutAsync(60000);
            }
            else
            {
                page = await context.NewPageAsync();
                //go to yandex market
                await page.GotoAsync("https://passport.yandex.ru/auth?mode=auth&retpath=https://partner.market.yandex.ru");
                await page.WaitForLoadStateAsync(LoadState.Load);

                //input email
                await Console.Out.WriteLineAsync($"\nstart login. {DateTime.Now}\n");

                while (true)
                    try
                    {
                        Console.WriteLine($"enter username. {DateTime.Now}");
                        await page.Locator("//input[@autocomplete = \"username\"]").WaitForAsync();
                        await page.Locator("//input[@autocomplete = \"username\"]").FillAsync(email);
                        break;
                    }
                    catch(TimeoutException)
                    {
                        Console.WriteLine($"try to open username tab. {DateTime.Now}");
                        var locator = page.GetByRole(AriaRole.Button).GetByText("Почта");
                        await locator.ClickAsync();

                        await page.Locator("//input[@autocomplete = \"username\"]").WaitForAsync();
                        await page.Locator("//input[@autocomplete = \"username\"]").FillAsync(email);
                        break;
                    }
                    catch (Exception ex)
                    {
                        await Console.Out.WriteLineAsync(ex.Message);
                    }

                //click enter
                while (true)
                    try
                    {
                        await Console.Out.WriteLineAsync($"\nsubmit. {DateTime.Now}\n");
                        await page.Locator("//button[@type = \"submit\" or contains(text(),'Войти')][1]").WaitForAsync();
                        await page.Locator("//button[@type = \"submit\" or contains(text(),'Войти')][1]").ClickAsync();
                        break;
                    }
                    catch { }

                //input the pass
                while (true)
                    try
                    {
                        await Console.Out.WriteLineAsync($"\nenter password. {DateTime.Now}\n");
                        await page.Locator("//input[contains(@placeholder,'Введите пароль')]").WaitForAsync();
                        await page.Locator("//input[contains(@placeholder,'Введите пароль')]").FillAsync(password);
                        break;
                    }
                    catch { }

                //click submit the pass
                while (true)
                    try
                    {
                        await Console.Out.WriteLineAsync($"\nsubmit. {DateTime.Now}\n");
                        await page.Locator("//button[@type = \"submit\" or contains(text(),'Войти')][1]").WaitForAsync();
                        await page.Locator("//button[@type = \"submit\" or contains(text(),'Войти')][1]").ClickAsync();
                        break;
                    }
                    catch { }

                //wait load page
                await page.WaitForLoadStateAsync(LoadState.Load);
                await page.WaitForTimeoutAsync(7000);

                //finde locator input with text = 'Восстановить доступ'
                var _input_text_restore = page.QuerySelectorAsync("//button/span[contains(text(),'Далее')]").Result;
                if (_input_text_restore is not null)
                {

                    var passFromPhone = await page.QuerySelectorAsync("//input");
                    if (passFromPhone is not null)
                    {
                        Console.Write("Введите пароль:");
                        var userInputPass = Console.ReadLine();
                        await passFromPhone.FillAsync(userInputPass);
                    }
                    else throw new Exception("Availible is closed: you need restore password");
                }
                await page.WaitForTimeoutAsync(10000);
                await Console.Out.WriteLineAsync($"\nsave context.json. {DateTime.Now}\n");
                await File.WriteAllTextAsync("context.json", context.StorageStateAsync().Result);
            }

            #endregion
        }
    }
}
