using Microsoft.AspNetCore.HttpOverrides;
using Scani.Kiosk.Backends.GoogleSheets;
using Scani.Kiosk.Services;

#pragma warning disable CA1812
var builder = WebApplication.CreateBuilder(args);
#pragma warning restore CA1812

// This is set from /profile/aspnetcore-defaults.sh in the dotnetcore-buildpack.
var isHeroku = Environment.GetEnvironmentVariable("IS_HEROKU")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

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

if (isHeroku)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });
}

var app = builder.Build();
var config = app.Services.GetRequiredService<IConfiguration>();

app.UseRequestLocalization(config.GetValue<string>("LocaleIdentifier"));

if (isHeroku)
{
    app.UseForwardedHeaders();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Heroku internally expects the server only to expect HTTP traffic because
    // the external load balancer/router performs SSL termination.
    if (!isHeroku)
    {
        app.UseHttpsRedirection();
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
