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

                TempData["SuccessMessage"] = "Data saved successfully."; // Simpan pesan sukses
                return RedirectToAction(nameof(Index)); // Mengarahkan kembali ke Index
            }

            // Jika validasi gagal, kembalikan ke halaman edit dengan model yang sama
            return View(gatewaySetting);
        }

        [HttpPost]
        public IActionResult ResetToDefault()
        {
            // Hapus setting yang ada (jika ada)
            var existingSetting = _dbContext.GatewaySettings.FirstOrDefault();
            if (existingSetting != null)
            {
                _dbContext.GatewaySettings.Remove(existingSetting);
                _dbContext.SaveChanges();
            }

            // Dapatkan atau buat pengaturan default
            GatewaySetting defaultSettings = SettingInitializer.GetOrCreateGatewaySetting(_dbContext);

            // Simpan pengaturan default ke basis data
            //_dbContext.Update(defaultSettings);
            //_dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Settings reset to default successfully."; // Simpan pesan sukses
            return RedirectToAction(nameof(Index)); // Mengarahkan kembali ke Index
        }

    }
}
