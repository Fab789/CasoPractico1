using EarthquakeApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IEarthquakeApiClient, EarthquakeApiClient>((sp, c) =>
{
    var baseUrl = builder.Configuration["EarthquakeApi:BaseUrl"] ?? "https://localhost:7100/";
    c.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
    c.Timeout = TimeSpan.FromSeconds(60);
});

var app = builder.Build();

// Nunca mostrar páginas de error 500 crudas
app.UseExceptionHandler("/Home/Error");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

