using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auren.Application.Common.Interfaces
{
    public interface IAuthDbContext
    {
        DbSet<ProfileUserImage> ProfileUserImages { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<ApplicationUser> Users { get; }
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
