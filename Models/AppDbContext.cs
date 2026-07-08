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
        }
}
    

