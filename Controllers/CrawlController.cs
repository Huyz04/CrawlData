﻿using CrawData.Model;
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
        public async Task<IActionResult> GetPapersFindPage(int typeId, int page)
        {
            if(page<=0) return BadRequest();
            var papers = await _sCrawl.GetPaperByType(typeId, page);
            if(papers == null)
            {
                return NotFound();
            }
            return Ok(papers);
        }

        [HttpGet("{paperId}")]
        public async Task<IActionResult> GetPaperFullContent(string paperId)
        {
            var paper = await _sCrawl.GetPaperFullContent(paperId);
            if(paper == null)
            {
                return NotFound();
            }
            return Ok(paper);
        }
    }
}
