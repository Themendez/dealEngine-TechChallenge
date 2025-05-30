using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace dealEngine.AmadeusFlightApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly IAmadeusService _amadeusService;
        public FlightsController(IAmadeusService amadeusService)
        {
            _amadeusService = amadeusService;
        }


        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            try
            {
                var token = await _amadeusService.GetTokenAsync();
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
    }
}
