using PlanetOverview.Services;
using PlanetOverview.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<INewsService, NewsService>();
builder.Services.AddSingleton<ICountryService, CountryService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("Countries", client =>
{
    client.BaseAddress = new Uri("https://api.restcountries.com/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("PlanetOverview/1.0");
});
builder.Services.AddHttpClient<ITimeService, TimeService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();
builder.Services.ConfigureHttpClientDefaults(client =>
{
    client.ConfigureHttpClient(http => http.Timeout = TimeSpan.FromSeconds(20));
    // Provider request URLs can contain API keys.
    client.RemoveAllLoggers();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Overview/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Overview}/{action=Index}/{id?}");

app.Run();
