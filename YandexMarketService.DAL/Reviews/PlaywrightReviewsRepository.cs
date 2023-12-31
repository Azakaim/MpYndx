using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Newtonsoft.Json;
using SharpSpin;
using YandexMarketService.BLL.CustomExceptions;
using YandexMarketService.BLL.Repositories;
using YandexMarketService.DAL.Models;

namespace YandexMarketService.DAL.Reviews
{
    public class PlaywrightReviewsRepository : IReviewsRepository
    {
        readonly IMemoryCache _memoryCache;
        readonly IConfiguration _configuration;

        public PlaywrightReviewsRepository(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public async Task HandleReviewsAsync(int userId)
        {
            string link_first_rew = String.Empty;
            string? templatesFilePath = _configuration["YANDEX_MARKET_TEMPLATES_FILE_PATH"];

            if (!_memoryCache.TryGetValue(userId, out IPage page))
                throw new CustomInvalidOperationException("Не удалось ответить на отзыв");

            //Read json file answer for reviews 
            string Json_Answer = string.Empty;
            try
            {
                using (var sr = new StreamReader(templatesFilePath))
                {
                    Json_Answer = await sr.ReadToEndAsync();
                    if (string.IsNullOrWhiteSpace(Json_Answer)) throw new Exception("Json is Empty");
                }
            }
            catch (Exception ex) { throw new Exception("File json is not excists :" + ex.Message); }

            //click on the button reviews
            while (true)
            {

                try
                {
                    var _el_convers = await page.QuerySelectorAsync("//span[contains(text(),'Общение')]//ancestor-or-self::a");

                    await _el_convers.HoverAsync();
                    await page.WaitForTimeoutAsync(1000);

                    //await page.Locator("//span[contains(text(),'Отзывы и')]//ancestor-or-self::a").WaitForAsync();
                    link_first_rew = await page.Locator("//span[contains(text(),'Отзывы и')]//ancestor-or-self::a").GetAttributeAsync("href");
                }
                catch { throw new Exception("Error! //span[contains(text(),'Общение' или 'Отзывы и')]"); }
                break;
            }

            //transition first rew
            await page.GotoAsync("https://partner.market.yandex.ru" + link_first_rew, new PageGotoOptions { Timeout = 60000 });
            await page.WaitForTimeoutAsync(5000);

            IEnumerable<IElementHandle> get_link_list;
            List<string> count_rev = new();
            //get nums of reviews
            while (true)
                try
                {

                    await page.Locator("//div[contains(@data-widget-name,'HeaderCampaigns')]//input").WaitForAsync();
                    var _el_input = await page.QuerySelectorAsync("//div[contains(@data-widget-name,'HeaderCampaigns')]//input");
                    await _el_input.ClickAsync();


                    //this clickable link 'Отзывы'
                    get_link_list = await page.QuerySelectorAllAsync("//div[contains(@data-id,'levitan-unified-popups-container')]//div[contains(@class,'content')]//a//*");
                    get_link_list = await page.QuerySelectorAllAsync("//div[contains(@data-id,'levitan-unified-popups-container')]//div[contains(@class,'content')]//a");//.GetAttributeAsync("href");

                    count_rev = get_link_list.Select(req => req.GetAttributeAsync("href").Result).Where(s => s.Contains("supplier")).ToList();

                    break;
                }
                catch { throw new Exception("Error! Code is not valid."); }

            //while (true)
            //    if (get_link != "" || get_link != null)
            //    {
            //        //wait for navigation
            //        await page.GotoAsync("https://vendor.market.yandex.ru" + get_link);

            //        break;
            //    }


            #region Process reviews


            //finde placeholder of input with companys
            //string _brands = string.Empty;
            //var input_placeholder = await page.QuerySelectorAsync("//input[@placeholder=\"Поиск\"]");
            //var List_Brands = await page.QuerySelectorAllAsync("//input[@placeholder='Поиск']/..//../*//div[contains(@class,'item')]");

            foreach (var get_link in count_rev)
            {
                //continue;
                await page.GotoAsync("https://partner.market.yandex.ru" + get_link, new PageGotoOptions { Timeout = 60000 });
                await page.WaitForTimeoutAsync(5000);

                var countPages = await page.QuerySelectorAsync("//div[contains(@class,'pages')]/button[last() - 1]//span[contains(@class,'ext')]");

                //var listButton = await page.QuerySelectorAllAsync("//div[contains(@class,'pages')]/button");

                //var listButtons = listButton.ToList();


                var pageNumNotConv = countPages?.InnerTextAsync().Result;

                int pageNum = 0;
                if (pageNumNotConv != null)
                {
                    pageNum = Convert.ToInt32(pageNumNotConv.Trim());
                }
                else break;


                for (int i = 2; i <= pageNum / 10; i++)
                {
                    //for scroll a page
                    var page_hight = await page.EvaluateAsync<int>(@"document.documentElement.scrollHeight");

                    for (int y = 0; y < 10; y++) await page.Mouse.WheelAsync(0, page_hight);
                    page_hight = await page.EvaluateAsync<int>(@"document.documentElement.scrollHeight");
                    Console.WriteLine("page_hight :" + page_hight);

                    //finde collection futer with reviews
                    IReadOnlyList<IElementHandle> html_coll = null;
                    while (true)
                        try
                        {
                            html_coll = await page.QuerySelectorAllAsync("//span[contains(text(),'Заказ:')]//ancestor-or-self::div[contains(@class,'style-card')]/..");
                            break;
                        }
                        catch { }



                    //html footer-collection without a answer body
                    List<IElementHandle> html_collect_footer_without_body_answer = new();
                    while (true)
                        try
                        {
                            foreach (var el in html_coll)
                            {
                                var answerFromCompany = await el.QuerySelectorAllAsync("//div[contains(@class,'style-shopAnswer')]");

                                if (answerFromCompany.Count != 0)
                                {
                                    continue;
                                    //var span_text = jsonArray.FirstOrDefault().QuerySelectorAllAsync("//div[contains(@class,'style-comment')]/span").Result;
                                    //var txt = span_text.FirstOrDefault().InnerTextAsync().Result;
                                }
                                else
                                {
                                    html_collect_footer_without_body_answer.Add(el);
                                }

                            }
                            break;
                        }
                        catch { }



                    //await page.PauseAsync();
                    Console.WriteLine("html_collection count: " + html_collect_footer_without_body_answer.Count);

                    //list Tuple< supplies, name of client, grade of supplies, comment positive, comment negative>  
                    List<Tuple<string, string, int, string, string, string>> list_tuple_product = new();
                    //create span click
                    IElementHandle? span_click = null;
                    //delete doubles
                    var html_collect_footer_without_body_answer_RESULT = html_collect_footer_without_body_answer.Distinct().ToList();
                    foreach (var el in html_collect_footer_without_body_answer_RESULT)
                    {
                        //it's button for answer
                        IReadOnlyList<IElementHandle>? answer_button = el.QuerySelectorAllAsync("button").Result;

                        //it's how much count of star
                        int _grade = Convert.ToInt32(el.QuerySelectorAllAsync("//div[@data-value]").Result.FirstOrDefault().GetAttributeAsync("data-value").Result);

                        //text reviews from client
                        var _komment = el.QuerySelectorAllAsync("//div[contains(@class,'style-userVerdictText')]").Result.FirstOrDefault().InnerTextAsync().Result;

                        //click to ansver
                        await answer_button.FirstOrDefault().ClickAsync();

                        //get textarea element for click
                        var placeholderForAnswer = el.QuerySelectorAllAsync("//textarea[contains(@placeholder,'Комментарий')]").Result;

                        //get name client
                        string _name_cl = el.QuerySelectorAllAsync("//div[contains(@class,'style-userInfo')]").Result.FirstOrDefault().InnerTextAsync().Result.Split(new char[] { '(', '-', ':', ';' }, StringSplitOptions.TrimEntries).FirstOrDefault();

                        //click on the button 'Ответить'
                        var buttonGoAnswer = el.QuerySelectorAllAsync("//button//span[contains(text(),'Ответить')]").Result;

                        //answer review
                        string _answer_review = string.Empty;

                        //fedback processing method with GPT-3

                        /*







                            ------------------Attention 
                        don't forget to change a state of work chat GPT 







                          */


                        if (_grade < 0)// ------------- (_grade < 3)
                        {
                            //_answer_review = _gptHelper.GPTHelperCreate(_name_cl, _prod, _grade, _positive, _negative, _komment, yandex.Name_Company);
                            Console.WriteLine($"GPT answer => {_answer_review}");
                        }
                        else
                        {

                            Root? myDeserializedClass = JsonConvert.DeserializeObject<Root>(Json_Answer);
                            if (_grade == 5)
                            {
                                var _answer = Spinner.Spin(myDeserializedClass?.star_5.ElementAt(new Random().Next(myDeserializedClass.star_5.Count())));
                                _answer_review = CreateAnswerSpintax(_answer, _name_cl);
                            }
                            else if (_grade == 4)
                            {
                                var _answer = Spinner.Spin(myDeserializedClass?.star_4.ElementAt(new Random().Next(myDeserializedClass.star_4.Count())));
                                _answer_review = CreateAnswerSpintax(_answer, _name_cl);

                            }
                            else if (_grade <= 3)// (_grade == 3)
                            {
                                var _answer = Spinner.Spin(myDeserializedClass?.bad_reviews.ElementAt(new Random().Next(myDeserializedClass.bad_reviews.Count())));
                                _answer_review = CreateAnswerSpintax(_answer, _name_cl);
                            }
                            Console.WriteLine("Spintax => :" + _answer_review);
                        }

                        try
                        {
                            //Click el span
                            await placeholderForAnswer.FirstOrDefault().WaitForElementStateAsync(ElementState.Visible);
                            await placeholderForAnswer.FirstOrDefault().TypeAsync(_answer_review);
                            await buttonGoAnswer.FirstOrDefault().FocusAsync();
                            await buttonGoAnswer.FirstOrDefault().ClickAsync();
                        }
                        catch (Exception ex) { continue; }


                        Console.WriteLine("++++++++++++++DONE++++++++++++++++++");
                        Thread.Sleep(5000);
                        Console.WriteLine("===============================");

                        //change page

                    }


                    //button for paginacia
                    var buttonForPaginacia = await page.QuerySelectorAsync($"//span[contains(@class,'ext') and text() = '{i}']/ancestor-or-self::button");

                    await buttonForPaginacia.ClickAsync();
                }


            }
            #endregion

            //await context.CloseAsync();
        }

        string CreateAnswerSpintax(string answer_review, string name)
        {
            var hi = "{Добрый день|Здравствуйте|Доброго времени суток|Приветствуем Вас}";
            var str1 = answer_review.Split('!').First();
            var witouth_str_1 = answer_review.Split('!', 2).Last();
            var str2 = Spinner.Spin(hi) + $", {name} " + "!";
            return str2 + witouth_str_1;
        }
    }
}
