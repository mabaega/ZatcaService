using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using ZatcaService.Helpers;
using ZatcaService.Models;
using ZatcaService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService();
builder.Services.AddWindowsService();

builder.Services.AddScoped<IGatewaySettingService, GatewaySettingService>();

builder.Services.Configure<JsonOptions>(options =>
         options.SerializerOptions.DefaultIgnoreCondition
   = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteConnection")));

//builder.Services.AddHttpClient<IRelayService, RelayService>();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
