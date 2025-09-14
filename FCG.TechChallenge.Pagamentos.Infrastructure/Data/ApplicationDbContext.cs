using FCG.TechChallenge.Pagamentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.TechChallenge.Pagamentos.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Payment> Payments => Set<Payment>();


        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Payment>(e =>
            {
                e.HasKey(p => p.Id);

                e.Property(p => p.MerchantId)
                 .IsRequired()
                 .HasMaxLength(100)                 
                 .HasColumnType("varchar(100)");

                e.Property(p => p.Method).HasConversion<int>().IsRequired();
                e.Property(p => p.Status).HasConversion<int>().IsRequired();

                e.Property(p => p.CreatedAtUtc).HasColumnType("datetime2");
                e.Property(p => p.CapturedAtUtc).HasColumnType("datetime2");
                e.Property(p => p.RefundedAtUtc).HasColumnType("datetime2");

                // Config do Value Object
                e.OwnsOne(p => p.Amount, money =>
                {
                    money.Property(m => m.Amount)
                         .HasColumnName("Amount")
                         .HasColumnType("decimal(18,2)");

                    money.Property(m => m.Currency)
                         .HasColumnName("Currency")
                         .HasMaxLength(3)
                         .IsFixedLength()
                         .IsRequired();
                });

                e.Navigation(p => p.Amount).IsRequired(); // garante NOT NULL
            });
        }
    }
}
