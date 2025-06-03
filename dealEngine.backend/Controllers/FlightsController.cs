using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Models;
using dealEngine.AmadeusFlightApi.Models.FligthOffer;
using dealEngine.AmadeusFlightApi.Models.Locations;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace dealEngine.AmadeusFlightApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly IAmadeusService _amadeusService;
        private readonly IAmadeusTokenService _tokenService;
        public FlightsController(IAmadeusService amadeusService, IAmadeusTokenService tokenService)
        {
            _amadeusService = amadeusService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Gets a token from Amadeus
        /// </summary>
        /// <returns>Bearer token string</returns>
        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            try
            {
                var token = await _tokenService.GetTokenAsync();
                return Ok(new ApiResponse<string> { Success = true, Data = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Error = ex.Message });
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchFlights(FlightPreference criteria)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (criteria == null)
            {
                return BadRequest("Flight criteria cannot be null.");
            }
            try
            {
                var flights = await _amadeusService.SearchFlightsAsync(criteria);
                return Ok(new ApiResponse<List<FlightResult>> { Success = true, Data = flights });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string> { Success = false, Error = ex.Message });
            }
        }


        [HttpPost("flight-offers")]
        public async Task<IActionResult> GetFlightOffers(FlightOfferRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var results = await _amadeusService.SearchFlightOffersAsync(request);
                return Ok(new ApiResponse<List<FlightOfferResult>> { Success = true, Data = results });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"External API call failed: {ex.Message}");
            }
        }


        [HttpPost("locations")]
        public async Task<IActionResult> GetLocations(LocationSearchRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var results = await _amadeusService.SearchLocationsAsync(request);
                return Ok(new ApiResponse<PagedResult<LocationResult>> { Success = true, Data = results });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"External API call failed: {ex.Message}");
            }
        }
    }
}
