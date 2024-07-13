using CrawData.DTO;
using CrawData.Model;

namespace CrawData.Responses
{
    public class ResponseListPaperWithType
    {
        public IEnumerable<PaperSummaryDTO> Collection { get; set; }
        public int Total { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public ResponseListPaperWithType(IEnumerable<PaperSummaryDTO> papers, int total = 0, int pageindex = 0)
        {
            this.Collection = papers;
            this.Total = total;
            this.PageSize = 10;
            this.PageIndex = pageindex;
        }
        public ResponseListPaperWithType()
        {
            this.Collection = new List<PaperSummaryDTO>();
            this.Total = 0;
            this.PageSize = 0;
            this.PageIndex = 0;
        }
    }
}
