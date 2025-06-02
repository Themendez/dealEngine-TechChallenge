namespace dealEngine.AmadeusFlightApi.Models.FligthOffer
{
    public class FlightOfferResult
    {
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Airline { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
    }
}
