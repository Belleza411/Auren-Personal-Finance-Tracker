using Auren.API.Models.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auren.API.Data
{
    public class AurenAuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public AurenAuthDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Goal> Goals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().HasAlternateKey(u => u.UserId);
            builder.Entity<Transaction>().HasKey(t => t.TransactionId);
            builder.Entity<Category>().HasKey(c => c.CategoryId);
            builder.Entity<Goal>().HasKey(g => g.GoalId);

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.RefreshTokenId);

                entity.Property(rt => rt.Token)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(rt => rt.UserId)
                      .IsRequired();

                entity.Property(rt => rt.ReplacedByToken)
                      .HasMaxLength(500);

                entity.Property(rt => rt.ReasonRevoked)
                      .HasMaxLength(200);

                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.UserId)
                      .HasPrincipalKey(u => u.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(rt => rt.Token)
                      .IsUnique();

                entity.HasIndex(rt => rt.UserId);

                entity.Property(rt => rt.IsActive)
                      .HasComputedColumnSql("CAST(CASE WHEN IsRevoked = 0 AND ExpiryDate > GETUTCDATE() THEN 1 ELSE 0 END AS BIT)");
            });
        }
    }
}
