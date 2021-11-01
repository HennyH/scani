using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Scani.Kiosk.Backends;
using Scani.Kiosk.Backends.GoogleSheet;
using Scani.Kiosk.Data;
using Scani.Kiosk.Services;
using Scani.Kiosk.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<ActiveUserService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IKioskBackend, GoogleSheetKioskBackend>();
builder.Services.AddSingleton<GoogleSheetSynchronizer>(s => new GoogleSheetSynchronizer(s.GetRequiredService<ILogger<GoogleSheetSynchronizer>>(), "1WT6kELSGHp6QUQo_K7KCSMyGMzYONt7Bzfpt5ETTO9I", TimeSpan.FromMinutes(1)));
builder.Services.AddHostedService<GoogleSheetSynchronizer>(s => s.GetRequiredService<GoogleSheetSynchronizer>());

var app = builder.Build();

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
