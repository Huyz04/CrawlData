using CrawData.Model;
using CrawData.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CrawData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrawlController : ControllerBase
    {
        private readonly SCrawl _sCrawl;
        public CrawlController(SCrawl sCrawl)
        {
            _sCrawl = sCrawl;
        }
        [HttpGet]
        public async Task<IActionResult> Crawl([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("URL is required");
            }

            try
            {
                var data = await _sCrawl.CrawlWebsiteAsync(url);
                return Ok(data);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500, "Error occurred while fetching data from the website.");
            }
        }
        [HttpGet("test")]
        public async Task<IActionResult> CrawlAuto()
        {
            try
            {
                var listLink = await _sCrawl.GetListType();
                var crawledPapers = new List<Paper>();

                foreach (var type in listLink)
                {
                    var papers = await _sCrawl.CrawlWebsiteAsync(type.Content);
                    crawledPapers.AddRange(papers);
                }

                return Ok(crawledPapers);
            }
            catch (HttpRequestException)
            {
                return StatusCode(500, "Error occurred while fetching data from the website.");
            }
        }
    }
}
