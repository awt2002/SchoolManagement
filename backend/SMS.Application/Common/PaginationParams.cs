namespace SMS.Application.Common
{
    public class PaginationParams
    {
        public int Page { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public int MaxPageSize { get; set; } = 100;
    }
}
