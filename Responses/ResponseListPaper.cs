namespace CrawData.Responses
{
    public class ResponseListPaper
    {
        public bool Success { get; set; }
        public ResponseListPaperWithType data { get; set; }
        public string Message { get; set; }
        public ResponseListPaper()
        {
            this.Success = false;
            this.data = new ResponseListPaperWithType();
        }
        public ResponseListPaper(bool success, ResponseListPaperWithType rePaper, string error)
        {
            this.Success = success;
            this.data = rePaper;
            this.Message = error;
        }
    }
}
