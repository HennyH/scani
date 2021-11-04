using Google.Apis.Sheets.v4;
using Scani.Kiosk.Backends.GoogleSheet;
using Scani.Kiosk.Helpers;
using Scani.Kiosk.Services;
using Scani.Kiosk.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<ActiveUserService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IKioskBackend, GoogleSheetKioskBackend>();
builder.Services.AddSingleton<GoogleSheetSynchronizer>();
builder.Services.AddSingleton<ThrottledKioskSheetAccessorFactory>();
builder.Services.AddSingleton<LazyAsyncThrottledAccessor<SheetsService>>(services =>
    services.GetRequiredService<ThrottledKioskSheetAccessorFactory>().CreateAccessor(100, TimeSpan.FromMinutes(1)));
builder.Services.AddSingleton<KioskSheetReaderWriter>();
builder.Services.AddSingleton<SynchronizedGoogleSheetKioskState>();
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
