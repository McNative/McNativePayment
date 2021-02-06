using Microsoft.EntityFrameworkCore;

namespace McNativePayment.Model
{
    public class McNativeContext : DbContext {

        public McNativeContext(DbContextOptions<McNativeContext> options) : base(options) {}

        public DbSet<LicenseActive> ActiveLicenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LicenseActive>().ToTable("mcnative_license_active");
        }

    }
}
