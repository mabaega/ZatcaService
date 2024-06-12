using Microsoft.AspNetCore.Mvc;
using ZatcaService.Models;
using ZatcaService.Helpers;

namespace ZatcaService.Controllers
{
    public class GatewaySettingsController : Controller
    {
        private readonly AppDbContext _dbContext;

        public GatewaySettingsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            GatewaySetting gatewaySetting = _dbContext.GatewaySettings.FirstOrDefault();
            return View(gatewaySetting);
        }

        [HttpPost]
        public IActionResult Edit(GatewaySetting gatewaySetting)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Update(gatewaySetting);
                _dbContext.SaveChanges();

                TempData["SuccessMessage"] = "Data saved successfully.";
                return RedirectToAction(nameof(Index)); 
            }

            return View(gatewaySetting);
        }

        [HttpPost]
        public IActionResult ResetToDefault()
        {
            var existingSetting = _dbContext.GatewaySettings.FirstOrDefault();
            if (existingSetting != null)
            {
                _dbContext.GatewaySettings.Remove(existingSetting);
                _dbContext.SaveChanges();
            }

            GatewaySetting defaultSettings = SettingInitializer.GetOrCreateGatewaySetting(_dbContext);

            _dbContext.Update(defaultSettings);
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Settings reset to default successfully."; 
            return RedirectToAction(nameof(Index));
        }

    }
}
