using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Transaction> Transactions { get; }
        DbSet<Category> Categories { get; }
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
