using Microsoft.EntityFrameworkCore;

namespace ZatcaService.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<GatewaySetting> GatewaySettings { get; set; }
        public DbSet<ApprovedInvoice> ApprovedInvoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}