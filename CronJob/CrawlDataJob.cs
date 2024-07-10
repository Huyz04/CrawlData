using Quartz;
using CrawData.Controllers;

namespace CrawData.CronJob
{
    public class CrawlDataJob: IJob
    {

        private readonly ILogger<CrawlDataJob> _logger;
        private readonly HttpClient _httpClient;
        public CrawlDataJob(ILogger<CrawlDataJob> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("CrawlJob is starting at {time}", DateTimeOffset.Now);

            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7240/api/Crawl/test");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("CrawlJob successfully called the API at {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CrawlJob failed to call the API at {time}", DateTimeOffset.Now);
            }
        }
    }
}
