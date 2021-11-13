using Microsoft.AspNetCore.Localization;
using Scani.Kiosk.Backends.GoogleSheet;
using Scani.Kiosk.Services;
using System.Globalization;

#pragma warning disable CA1812
var builder = WebApplication.CreateBuilder(args);
#pragma warning restore CA1812

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<ActiveUserService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<KioskSheetSynchronizer>();
builder.Services.AddSingleton<ThrottledKioskSheetAccessor>();
builder.Services.AddSingleton<KioskSheetReaderWriter>();
builder.Services.AddSingleton<SynchronizedKioskState>();
builder.Services.AddHostedService<KioskSheetSynchronizer>();

var app = builder.Build();
var config = app.Services.GetRequiredService<IConfiguration>();

app.UseRequestLocalization(options =>
{
    var localeIdentifier = config.GetValue<string?>("DefaultLocaleIdentifier");
    if (!string.IsNullOrWhiteSpace(localeIdentifier))
    {
        options.DefaultRequestCulture = new RequestCulture(localeIdentifier);
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
