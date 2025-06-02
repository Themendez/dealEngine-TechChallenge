namespace dealEngine.AmadeusFlightApi.Models.FligthOffer
{
    public class OriginDestination
    {
        public string Id { get; set; } = "1";
        public string OriginLocationCode { get; set; } = string.Empty;
        public string DestinationLocationCode { get; set; } = string.Empty;
        public DepartureDateTimeRange DepartureDateTimeRange { get; set; } = new();
    }
}
