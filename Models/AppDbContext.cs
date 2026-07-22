using Microsoft.EntityFrameworkCore;

namespace CasaPuritaRMS.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Unit> Units { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<RentPayment> RentPayments { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Deleting a unit sets tenant Unit_ID to null.
            modelBuilder.Entity<Tenant>()
                .HasOne(t => t.Unit)
                .WithMany(u => u.Tenants)
                .HasForeignKey(t => t.Unit_ID)
                .OnDelete(DeleteBehavior.SetNull);

            // Deleting a tenant cascades to their payments.
            modelBuilder.Entity<RentPayment>()
                .HasOne(r => r.Tenant)
                .WithMany()
                .HasForeignKey(r => r.Tenant_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // Deleting a tenant cascades to their maintenance requests.
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Tenant)
                .WithMany()
                .HasForeignKey(m => m.Tenant_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // Deleting a tenant keeps notifications but clears the link.
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Tenant)
                .WithMany()
                .HasForeignKey(n => n.Tenant_ID)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}