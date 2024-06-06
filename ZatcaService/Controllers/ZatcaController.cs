using Microsoft.AspNetCore.Mvc;

namespace ZatcaService.Controllers
{
    public class ZatcaController : Controller
    {
        public IActionResult RelaySetting()
        {
            return View();
        }

        public IActionResult SignedInvoices()
        {
            return View();
        }
    }
}
