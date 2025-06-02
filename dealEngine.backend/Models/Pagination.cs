namespace dealEngine.AmadeusFlightApi.Models
{
    public abstract class Pagination
    {
        private const int MaxPageSize = 100;

        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }


        public int GetOffset()
        {
            return (PageNumber - 1) * PageSize;
        }
    }
}
