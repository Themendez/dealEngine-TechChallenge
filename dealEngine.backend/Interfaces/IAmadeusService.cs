using dealEngine.AmadeusFlightApi.Models;

namespace dealEngine.AmadeusFlightApi.Interfaces
{
    public interface IAmadeusService
    {
        Task<string> GetTokenAsync();
        Task<List<FlightResult>> SearchFlightsAsync(FlightPreference criteria);
    }
}
