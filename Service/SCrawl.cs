using CrawData.Data;
using CrawData.Model;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Serilog;
using System.Configuration;
using System.Drawing.Printing;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CrawData.Service
{
    public class SCrawl
    {
        private readonly DataContext _context;
        public SCrawl(HttpClient httpClient, DataContext context)
        {
            _context = context; 
        }
        public async Task<List<Paper>> CrawlWebsiteAsync(string url)
        {
            var papers = new List<Paper>();

            try
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                var page = await browser.NewPageAsync();
                await page.GotoAsync(url, new PageGotoOptions { Timeout = 50000 });

                await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 50000 });

                var content = await page.ContentAsync();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);

                var listingMainDiv = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'list__listing-main')]");
                if (listingMainDiv == null)
                {
                    return papers;
                }

                var boxCategoryItems = listingMainDiv.SelectNodes(".//div[contains(@class, 'box-category-item')]");
                if (boxCategoryItems == null)
                {
                    return papers;
                }

                var type = _context.Types.FirstOrDefault(t => t.Content == url);

                foreach (var item in boxCategoryItems)
                {
                    var hrefNode = item.SelectSingleNode(".//a[contains(@href, '.htm')]");
                    if (hrefNode != null)
                    {
                        var href = hrefNode.GetAttributeValue("href", string.Empty);
                        var id = href.Split('-').Last().Replace(".htm", "");
                        string urlpaper = "https://tuoitre.vn" + href;
                        var fullContent = await GetFullContentAsync(browser, urlpaper);

                        var paper = new Paper
                        {
                            Id = id,
                            Content = item.OuterHtml,
                            FullContent = fullContent,
                            typee = type
                        };

                        await SavePaperToDatabase(paper);
                        papers.Add(paper);
                        Log.Information($"Crawled paper with ID {id} from URL: {urlpaper}");
                    }
                }
                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                // Ghi lại các lỗi và tiếp tục
                Console.WriteLine($"Unexpected error: {ex.Message} for URL: {url}");
                Log.Error(ex, $"Error crawling website for URL: {url}");
            }

            return papers;
        }

        private async Task<string> GetFullContentAsync(IBrowser browser, string url)
        {
            const int maxRetries = 2; // Số lần thử lại tối đa
            const int delayMilliseconds = 2000; // Thời gian chờ giữa các lần thử lại

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                    var response = await httpClient.GetAsync(url);

                    // Kiểm tra mã trạng thái HTTP
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error: Received status code {(int)response.StatusCode} ({response.ReasonPhrase}) for URL: {url}");
                        Log.Error($"Received status code {(int)response.StatusCode} ({response.ReasonPhrase}) for URL: {url}");
                        await Task.Delay(delayMilliseconds); // Thời gian chờ trước khi thử lại
                        continue; // Thử lại yêu cầu
                    }

                    var html = await response.Content.ReadAsStringAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    // Tìm các phần tử cụ thể trong #main-detail
                    var mainDetail = doc.DocumentNode.SelectSingleNode("//*[@id='main-detail']");
                    if (mainDetail != null)
                    {
                        var detailTop = mainDetail.SelectSingleNode("//div[@class='detail-top']");
                        var detailTitle = mainDetail.SelectSingleNode("//h1[@class='detail-title article-title']");
                        var detailSapo = mainDetail.SelectSingleNode("//div[@class='detail-sapo']");
                        var detailMain = mainDetail.SelectSingleNode("//div[@class='detail-cmain clearfix']");

                        // Kết hợp tất cả các phần tử HTML ngoài được chọn
                        var combinedHtml = string.Join(Environment.NewLine,
                            new[] { detailTop, detailTitle, detailSapo, detailMain }
                                .Where(el => el != null)
                                .Select(el => el.OuterHtml));

                        // Gói HTML kết hợp vào một <div> mới
                        var result = $"<div>{combinedHtml}</div>";

                        return result;
                    }

                    return string.Empty;
                }
                catch (HttpRequestException ex)
                {
                    // Ghi lại lỗi HTTP và tiếp tục
                    Console.WriteLine($"HTTP request error: {ex.Message} for URL: {url}");
                    Log.Error($"HTTP request error: {ex.Message} for URL: {url}");
                    await Task.Delay(delayMilliseconds); // Thời gian chờ trước khi thử lại
                }
                catch (Exception ex)
                {
                    // Ghi lại các lỗi khác và tiếp tục
                    Console.WriteLine($"Unexpected error: {ex.Message} for URL: {url}");
                    Log.Error(ex, $"Unexpected error for URL: {url}");
                    await Task.Delay(delayMilliseconds); // Thời gian chờ trước khi thử lại
                }
            }

            // Nếu tất cả các lần thử đều thất bại, trả về chuỗi rỗng
            return string.Empty;
        }

        private async Task SavePaperToDatabase(Paper paper)
        {
            var existingPaper = await _context.Papers.FindAsync(paper.Id);
            if (existingPaper == null)
            {
                _context.Papers.Add(paper);
            }
            else
            {
                existingPaper.Content = paper.Content;
                existingPaper.FullContent = paper.FullContent;
                _context.Papers.Update(existingPaper);
            }
            await _context.SaveChangesAsync();
        }
        public async Task<List<Typee>> CreateType(Typee typee)
        {
                _context.Types.Add(typee);
               await _context.SaveChangesAsync();
                return await _context.Types.ToListAsync();
        }
        public async Task<List<Typee>> GetListType()
        {
            return await _context.Types.ToListAsync();
        }

        public async Task<IEnumerable<string>> GetPaperByType(int typeId, int page)
        {
            var papers = await _context.Papers
                                       .Include(p => p.typee)
                                       .Where(p => p.typee.Id == typeId)
                                       .Select(p=>p.Content)
                                        .Skip((page - 1) * 10)
                                        .Take(10)
                                       .ToListAsync();

            if (papers == null || papers.Count == 0)
            {
                return null;
            }

            return papers;
        }

        public async Task<string> GetPaperFullContent(string paperId)
        {
            var paper = await _context.Papers
                .Where(p => p.Id == paperId).
                Select(paper=>paper.FullContent)
                .FirstOrDefaultAsync();

            if(paper == null)
            {
                return null;
            }
          
            return paper;
        }
    }
}
