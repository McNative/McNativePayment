using Microsoft.EntityFrameworkCore;

namespace McNativePayment.Model
{
    public class PaymentContext : DbContext {

        public PaymentContext(DbContextOptions<PaymentContext> options) : base(options){}

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderProduct> OrderProducts { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductEdition> ProductEditions { get; set; }

        public DbSet<ProductAssignment> ProductAssignments { get; set; }

        public DbSet<Issuer> Issuers { get; set; }

        public DbSet<Referral> Referrals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>().ToTable("mcnative_payment_transactions");
            modelBuilder.Entity<Order>().ToTable("mcnative_payment_order");
            modelBuilder.Entity<OrderProduct>().ToTable("mcnative_payment_order_products");
            modelBuilder.Entity<Product>().ToTable("mcnative_payment_product");
            modelBuilder.Entity<ProductEdition>().ToTable("mcnative_payment_product_editions");
            modelBuilder.Entity<ProductAssignment>().ToTable("mcnative_payment_product_assignments");
            modelBuilder.Entity<Issuer>().ToTable("mcnative_payment_issuer");
            modelBuilder.Entity<Referral>().ToTable("mcnative_payment_referral");
        }

    }
}
