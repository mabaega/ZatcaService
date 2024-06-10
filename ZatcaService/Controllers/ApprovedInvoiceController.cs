using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZatcaService.Models;

namespace ZatcaService.Controllers
{
    public class ApprovedInvoiceController : Controller
    {
        private readonly AppDbContext _context;

        public ApprovedInvoiceController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index() // Ubah tindakan menjadi "Index"
        {
            var invoices = await _context.ApprovedInvoices.ToListAsync();
            return View(invoices);
        }
    }

}