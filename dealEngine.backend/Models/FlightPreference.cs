using System.ComponentModel.DataAnnotations;

namespace dealEngine.AmadeusFlightApi.Models
{
    public class FlightPreference
    {
        [Required(ErrorMessage = "Origin is required")]
        [MinLength(1, ErrorMessage = "Origin cannot be empty")]
        public string Origin { get; set; } = string.Empty;
        public string DepartureDate { get; set; } = string.Empty;
        public bool OneWay { get; set; } = false;
        public bool NonStop { get; set; } = false;
        public int MaxPrice { get; set; }
        public ViewByEnum ViewBy { get; set; }
    }
}
