using AutoMapper;
using dealEngine.AmadeusFlightApi;
using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AmadeusSettings>(builder.Configuration.GetSection("Amadeus"));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddLogging();

builder.Services.AddControllers();

//builder.Services.AddHttpClient<IAmadeusTokenService, AmadeusTokenService>();
builder.Services.AddScoped<IAmadeusTokenService, AmadeusTokenService>();

builder.Services.AddHttpClient("AmadeusClient")
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(RetryPolicyProvider.GetRetryPolicy());

//builder.Services.AddHttpClient<IAmadeusService, AmadeusService>();

builder.Services.AddScoped<IAmadeusService>(provider =>
{
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    var client = factory.CreateClient("AmadeusClient");

    var config = provider.GetRequiredService<IConfiguration>();
    var tokenService = provider.GetRequiredService<IAmadeusTokenService>();
    var mapper = provider.GetRequiredService<IMapper>();
    var logger = provider.GetRequiredService<ILogger<AmadeusService>>();

    return new AmadeusService(client, config, tokenService, mapper, logger);
});


builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
