using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auren.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Transaction> Transactions { get; }
        DbSet<Category> Categories { get; }
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
