using Auren.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Auren.API.Data
{
    public class AurenDbContext : DbContext
    {
        public AurenDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Goal> Goals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>().HasKey(t => t.TransactionId);
            builder.Entity<Category>().HasKey(c => c.CategoryId);
            builder.Entity<Goal>().HasKey(g => g.GoalId);
        }
    }
}
