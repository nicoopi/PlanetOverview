using Microsoft.AspNetCore.Localization;
using System.Globalization;
using PlanetOverview.Services;
using PlanetOverview.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Adiciona o serviço de localização informando a pasta "Resources"
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// 2. Modifica os Controllers e Views para suportarem arquivos de tradução
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix);

builder.Services.AddHttpClient<INewsService, NewsService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<ICountryService, CountryService>();
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

// 3. Define os idiomas suportados (Português do Brasil como padrão e Inglês)
var supportedCultures = new[]
{
    new CultureInfo("pt-BR"),
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

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