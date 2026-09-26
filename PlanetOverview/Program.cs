using PlanetOverview.Services;
using PlanetOverview.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<INewsService, NewsService>();
builder.Services.AddHttpClient<ICountryService, CountryService>();
builder.Services.AddHttpClient<ITimeService, TimeService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Overview}/{action=Index}/{id?}");

app.Run();