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
        /// Obtiene un token de acceso de Amadeus API.
        /// </summary>
        [HttpGet("token")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Busca vuelos disponibles según los criterios indicados.
        /// </summary>
        [HttpPost("search")]
        [ProducesResponseType(typeof(ApiResponse<List<FlightResult>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
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
                return BadRequest(new ApiResponse<string> { Success = false, Error = ex.Message });
            }
        }

        /// <summary>
        /// Busca ofertas de vuelos según los criterios de preferencia de viaje.
        /// </summary>
        [HttpPost("flight-offers")]
        [ProducesResponseType(typeof(ApiResponse<List<FlightOfferResult>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
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
                return BadRequest(new ApiResponse<string> { Success = false, Error = ex.Message });
            }
        }


        /// <summary>
        /// Busca aeropuertos y ciudades según el criterio proporcionado.
        /// </summary>
        /// <param name="request">Parámetros de búsqueda: palabra clave, país, tipo, orden, etc.</param>
        /// <returns>Lista paginada de ubicaciones coincidentes.</returns>
        [HttpPost("locations")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LocationResult>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
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
                return BadRequest (new ApiResponse<string> { Success = false, Error = ex.Message });
            }
        }
    }
}
