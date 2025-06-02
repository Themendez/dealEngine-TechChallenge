namespace dealEngine.AmadeusFlightApi.Models.FligthOffer
{
    public class FlightOfferRequest
    {
        public string CurrencyCode { get; set; } = "USD";
        public List<OriginDestination> OriginDestinations { get; set; } = new();
        public List<Traveler> Travelers { get; set; } = new();
        public List<string> Sources { get; set; } = new() { "GDS" };
        public SearchCriteria SearchCriteria { get; set; } = new();
    }
}
