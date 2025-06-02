namespace dealEngine.AmadeusFlightApi.Models.FligthOffer
{
    public class SearchCriteria
    {
        public int MaxFlightOffers { get; set; } = 2;
        public FlightFilters FlightFilters { get; set; } = new();
    }
}
