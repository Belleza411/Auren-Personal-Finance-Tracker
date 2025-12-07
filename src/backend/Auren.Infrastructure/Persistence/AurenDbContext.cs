using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auren.Infrastructure.Persistence
{
    public class AurenDbContext : DbContext
    {
        public AurenDbContext(DbContextOptions<AurenDbContext> options) : base(options)
        {
        }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Goal> Goals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>().HasKey(t => t.TransactionId);

            builder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Amount)
                    .HasPrecision(12, 2);
            });
                
            builder.Entity<Category>().HasKey(c => c.CategoryId);

            builder.Entity<Goal>().HasKey(g => g.GoalId);

            builder.Entity<Goal>(entity =>
            {
                entity.Property(g => g.Spent)
                    .HasPrecision(12, 2);

                entity.Property(g => g.Budget)
                    .HasPrecision(12, 2);
            });
        }
    }
}
