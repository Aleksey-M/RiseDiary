namespace RiseDiary.Model
{
    public class RecordsTextFilter
    {
        public string? SearchText { get; set; }

        private int _pageSize = 20;
        private int _pageNo; 

        public int PageSize 
        { 
            get => _pageSize;
            set => _pageSize = value > 0 && value < 100 ? value : 20;
        }

        public int PageNo 
        { 
            get => _pageNo;
            set => _pageNo = value >= 0 ? value : 0;
           
        }
    }
}
