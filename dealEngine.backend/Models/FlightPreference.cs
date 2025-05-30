namespace dealEngine.AmadeusFlightApi.Models
{
    public class FlightPreference
    {
        public string Origin { get; set; } = "PAR";
        public int MaxPrice { get; set; } = 500;
        public string SortBy { get; set; } = "price";
    }
}
