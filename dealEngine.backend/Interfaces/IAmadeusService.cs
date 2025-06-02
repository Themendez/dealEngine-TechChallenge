using dealEngine.AmadeusFlightApi.Models;
using dealEngine.AmadeusFlightApi.Models.FligthOffer;
using dealEngine.AmadeusFlightApi.Models.Locations;
using Microsoft.AspNetCore.Mvc;

namespace dealEngine.AmadeusFlightApi.Interfaces
{
    public interface IAmadeusService
    {
        Task<string> GetTokenAsync();
        Task<List<FlightResult>> SearchFlightsAsync(FlightPreference criteria);
        Task<List<FlightOfferResult>> SearchFlightOffersAsync(FlightOfferRequest request);
        Task<PagedResult<LocationResult>> SearchLocationsAsync(LocationSearchRequest request);
    }
}
