using CrawData.Model;
using CrawData.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CrawData.Responses;

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
        [HttpGet("test")]
        public async Task<IActionResult> CrawlAuto()
        {
            var listLink = await _sCrawl.GetListType();

            foreach (var type in listLink)
            {
                var papers = await _sCrawl.CrawlWebsiteAsync(type.Content);
            }

            return Ok("Success");
        }

        [HttpGet("{typeId}/{page}")]
        public async Task<ActionResult<ResponseListPaper>> GetPapersFindPage(int typeId, int page)
        {
            return await _sCrawl.GetPaperByType(typeId, page);
        }

        [HttpGet("{paperId}")]
        public async Task<ActionResult<ResponsePaper>> GetPaperFullContent(string paperId)
        {
            return await _sCrawl.GetPaperFullContent(paperId);
        }
    }
}
