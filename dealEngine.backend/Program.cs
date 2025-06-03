using dealEngine.AmadeusFlightApi;
using dealEngine.AmadeusFlightApi.Interfaces;
using dealEngine.AmadeusFlightApi.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.Configure<AmadeusSettings>(builder.Configuration.GetSection("Amadeus"));
builder.Services.AddHttpClient<IAmadeusTokenService, AmadeusTokenService>();

builder.Services.AddHttpClient("AmadeusClient")
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(RetryPolicyProvider.GetRetryPolicy());

builder.Services.AddHttpClient<IAmadeusService, AmadeusService>();


builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));

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
