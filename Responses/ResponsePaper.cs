using CrawData.DTO;

namespace CrawData.Responses
{
    public class ResponsePaper
    {
        public bool Success { get; set; }
        public string data { get; set; }
        public string Message { get; set; }
        public ResponsePaper()
        {
            this.Success = false;
            this.data = new string("");
        }
        public ResponsePaper(bool success, string rePaper, string error)
        {
            this.Success = success;
            this.data = rePaper;
            this.Message = error;
        }
    }
}
