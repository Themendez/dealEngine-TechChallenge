using System.ComponentModel.DataAnnotations;

namespace dealEngine.AmadeusFlightApi.Models.Locations
{
    public class LocationSearchRequest:Pagination
    {
        [Required]
        public string Keyword { get; set; } = string.Empty;

        public string SubType { get; set; } = "CITY";

        public string CountryCode { get; set; } = string.Empty;

        public string Sort { get; set; } = "analytics.travelers.score";

        public string View { get; set; } = "FULL";
    }
}
