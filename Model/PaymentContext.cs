using Microsoft.EntityFrameworkCore;

namespace McNativePayment.Model
{
    public class PaymentContext : DbContext {

        public PaymentContext(DbContextOptions<PaymentContext> options) : base(options){}

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Request> Requests { get; set; }

        public DbSet<Issuer> Issuers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>().ToTable("mcnative_payment_transactions");
            modelBuilder.Entity<Request>().ToTable("mcnative_payment_requests");
            modelBuilder.Entity<Issuer>().ToTable("mcnative_payment_issuers");
        }

    }
}
