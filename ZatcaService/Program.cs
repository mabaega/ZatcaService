using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json.Serialization;
using ZatcaService.Helpers;
using ZatcaService.Models;
using ZatcaService.Services;

var builder = WebApplication.CreateSlimBuilder(args);

var portArg = args.FirstOrDefault(arg => arg.StartsWith("-p="));
var dbPathArg = args.FirstOrDefault(arg => arg.StartsWith("-d="));

int port = 4455;
string dbPath = Path.Combine(AppContext.BaseDirectory, "ZatcaInvoice.db");

if (portArg != null && int.TryParse(portArg.Split('=')[1], out int parsedPort))
{
    port = parsedPort;
}

if (dbPathArg != null)
{
    dbPath = dbPathArg.Split('=')[1];
}

var dbDirectory = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

dbPath = $"Data Source={dbPath}";

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(port);
    //options.ListenAnyIP(5001, listenOptions =>
    //{
    //    listenOptions.UseHttps(); 
    //});
});

builder.Host.UseWindowsService();
builder.Services.AddWindowsService();

builder.Services.AddScoped<IGatewaySettingService, GatewaySettingService>();

builder.Services.Configure<JsonOptions>(options =>
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(dbPath));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    _dbContext.Database.EnsureCreated();

    var gatewaySetting = SettingInitializer.GetOrCreateGatewaySetting(_dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var defaultCulture = new CultureInfo("en-US");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

app.UseRequestLocalization(localizationOptions);

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
