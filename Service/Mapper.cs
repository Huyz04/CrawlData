using static System.Runtime.InteropServices.JavaScript.JSType;
using AutoMapper;

namespace CrawData.Service
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<Model.Paper, DTO.FullContentPaperDTO>();
            CreateMap<Model.Paper, DTO.PaperSummaryDTO>();
        }
    }
}
