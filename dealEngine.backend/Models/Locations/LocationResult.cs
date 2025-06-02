namespace dealEngine.AmadeusFlightApi.Models.Locations
{
    public class LocationResult
    {
        public string IataCode { get; set; }
        public string Name { get; set; }
        public string SubType { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int? TravelerScore { get; set; }
    }
}
