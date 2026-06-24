using Auren.Application.Common.Interfaces;
using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auren.Infrastructure.Persistence
{
    public class AurenDbContext : DbContext, IAppDbContext
    {
        public AurenDbContext(DbContextOptions<AurenDbContext> options) : base(options)
        {
        }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>().HasKey(t => t.Id);

            builder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Amount)
                    .HasPrecision(12, 2);
            });

            builder.Entity<Transaction>()
                .HasIndex(t => new { t.UserId, t.TransactionDate });

            builder.Entity<Transaction>()
                .HasIndex(t => t.CategoryId);

            builder.Entity<Category>().HasKey(c => c.Id);
        }
    }
}
