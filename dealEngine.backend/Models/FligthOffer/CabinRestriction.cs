namespace dealEngine.AmadeusFlightApi.Models.FligthOffer
{
    public class CabinRestriction
    {
        public string Cabin { get; set; } = "BUSINESS";
        public string Coverage { get; set; } = "MOST_SEGMENTS";
        public List<string> OriginDestinationIds { get; set; } = new() { "1" };
    }
}
