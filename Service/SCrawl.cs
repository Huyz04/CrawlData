using CrawData.Data;
using CrawData.Model;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace CrawData.Service
{
    public class SCrawl
    {
        private readonly HttpClient _httpClient;
        private readonly DataContext _context;
        public SCrawl(HttpClient httpClient, DataContext context)
        {
            _httpClient = httpClient;
            _context = context; 
        }

        public async Task<List<Paper>> CrawlWebsiteAsync(string url)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await page.GotoAsync(url);

            // Đợi cho đến khi tất cả các nội dung đã được tải
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Lấy toàn bộ nội dung HTML của trang
            var content = await page.ContentAsync();

            // Đóng trình duyệt
            await browser.CloseAsync();

            // Load HTML content using HtmlAgilityPack
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            // Select the parent div with class "list__listing-main"
            var listingMainDiv = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'list__listing-main')]");
            if (listingMainDiv == null)
            {
                return new List<Paper>();
            }

            // Select all child divs with class "box-category-item"
            var boxCategoryItems = listingMainDiv.SelectNodes(".//div[contains(@class, 'box-category-item')]");
            if (boxCategoryItems == null)
            {
                return new List<Paper>();
            }
            var type = new Typee();
            type = _context.Types.Where(t => t.Content == url).FirstOrDefault();
            // Extract IDs and div content
            var papers = new List<Paper>();
            foreach (var item in boxCategoryItems)
            {
                var hrefNode = item.SelectSingleNode(".//a[contains(@href, '.htm')]");
                if (hrefNode != null)
                {
                    var href = hrefNode.GetAttributeValue("href", string.Empty);
                    var id = href.Split('-').Last().Replace(".htm", "");
                    var paper = new Paper
                    {
                        Id = id,
                        Content = item.OuterHtml,
                        typee = type
                    };
                    papers.Add(paper);
                }
            }
            await SavePapersToDatabase(papers);
            return papers;
        }

        private string ExtractData(HtmlDocument document)
        {
            // Your data extraction logic here
            // For example, extract all paragraph texts
            var paragraphs = document.DocumentNode.SelectNodes("//p");
            var data = string.Join("\n", paragraphs.Select(p => p.InnerText));
            return data;
        }
        private async Task SavePapersToDatabase(List<Paper> papers)
        {
            foreach (var paper in papers)
            {
                var existingPaper = await _context.Papers.FindAsync(paper.Id);
                if (existingPaper == null)
                {
                    _context.Papers.Add(paper);
                }
                else
                {
                    existingPaper.Content = paper.Content;
                    _context.Papers.Update(existingPaper);
                }
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
    }
}
