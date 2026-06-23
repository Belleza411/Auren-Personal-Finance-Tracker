using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Common.Interfaces
{
    public interface IAuthDbContext
    {
        DbSet<ProfileUserImage> ProfileUserImages { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
