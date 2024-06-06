using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ZatcaService.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<SignedInvoice> SignedInvoices { get; set; }
    }
}
