namespace dealEngine.AmadeusFlightApi.Interfaces
{
    public interface IAmadeusTokenService
    {
        Task<string> GetTokenAsync();
        Task<string> GetTokenAsync(bool forceRefresh);
    }
}
