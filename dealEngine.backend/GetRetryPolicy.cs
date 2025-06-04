using dealEngine.AmadeusFlightApi.Services;
using Polly;
using Polly.Extensions.Http;

namespace dealEngine.AmadeusFlightApi
{
    public static class RetryPolicyProvider
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {

            return HttpPolicyExtensions.HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        
                        Console.WriteLine($"---- Retry {retryCount} encountered an error: {exception?.Exception}. Waiting {timeSpan} before next retry. ----");
                    });
        }
    }
}
