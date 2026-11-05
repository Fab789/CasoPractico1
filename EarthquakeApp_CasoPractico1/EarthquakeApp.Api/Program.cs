using EarthquakeApp.Api.Middleware;
using EarthquakeApp.Api.Models;
using EarthquakeApp.Api.Services;
using EarthquakeApp.Data;
using EarthquakeApp.Data.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Secrets / API keys: siempre desde appsettings.json
builder.Services.Configure<UsgsOptions>(builder.Configuration.GetSection("Usgs"));
builder.Services.Configure<RestCountriesOptions>(builder.Configuration.GetSection("RestCountries"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Entity Framework + Repository
builder.Services.AddDbContext<EarthquakeDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("EarthquakeDb")));
builder.Services.AddScoped<IEarthquakeRepository, EarthquakeRepository>();

// Memory cache
builder.Services.AddMemoryCache();

// Servicios que llaman a APIs externas
builder.Services.AddHttpClient<IUsgsService, UsgsService>(c => c.Timeout = TimeSpan.FromMinutes(2));
builder.Services.AddHttpClient<IRestCountriesService, RestCountriesService>((sp, c) =>
{
    var baseUrl = builder.Configuration["RestCountries:BaseUrl"] ?? "https://api.restcountries.com/countries/v5/";
    c.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
    c.Timeout = TimeSpan.FromSeconds(20);
});

builder.Services.AddScoped<IEarthquakeSyncService, EarthquakeSyncService>();
builder.Services.AddScoped<IEarthquakeSearchService, EarthquakeSearchService>();
builder.Services.AddHostedService<EarthquakeSeederHostedService>();

var app = builder.Build();

// Crea la BD/tablas si no existen (alternativa: ejecutar Database/CreateDatabase.sql)
using (var scope = app.Services.CreateScope())
{
    try
    {
        scope.ServiceProvider.GetRequiredService<EarthquakeDbContext>().Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "No se pudo conectar/crear la base de datos.");
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
