using AutoMapper;
using dealEngine.AmadeusFlightApi.Models.Locations;
using System.Runtime;
using System.Text.Json;

namespace dealEngine.AmadeusFlightApi.Models.Automapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<JsonElement, LocationResult>()
                .ForMember(dest => dest.IataCode, opt => opt.MapFrom(src => src.GetProperty("iataCode").GetString()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GetProperty("name").GetString()))
                .ForMember(dest => dest.SubType, opt => opt.MapFrom(src => src.GetProperty("subType").GetString()))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.GetProperty("geoCode").GetProperty("latitude").GetDouble()))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.GetProperty("geoCode").GetProperty("longitude").GetDouble()))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.GetProperty("address").GetProperty("cityName").GetString()))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.GetProperty("address").GetProperty("countryName").GetString()))
                .ForMember(dest => dest.TravelerScore, opt => opt.MapFrom((src, dest) =>
                {
                    if (src.TryGetProperty("analytics", out var analytics) &&
                        analytics.TryGetProperty("travelers", out var travelers) &&
                        travelers.TryGetProperty("score", out var score))
                    {
                        return score.GetInt32();
                    }
                    return (int?)null;
                }));
        }
    }
}
