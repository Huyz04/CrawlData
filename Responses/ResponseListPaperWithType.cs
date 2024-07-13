using CrawData.Model;

namespace CrawData.Responses
{
    public class ResponseListPaperWithType
    {
        public IEnumerable<string> Collection { get; set; }
        public int Total { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public ResponseListPaperWithType(IEnumerable<string> papers, int total = 0, int pageindex = 0)
        {
            this.Collection = papers;
            this.Total = total;
            this.PageSize = 10;
            this.PageIndex = pageindex;
        }
        public ResponseListPaperWithType()
        {
            this.Collection = new List<string>();
            this.Total = 0;
            this.PageSize = 0;
            this.PageIndex = 0;
        }
    }
}
