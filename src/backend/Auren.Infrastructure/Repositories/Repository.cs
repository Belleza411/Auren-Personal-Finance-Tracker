using Auren.Application.DTOs.Responses;
using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Common;
using Auren.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auren.Infrastructure.Repositories
{
	public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
	{
		private readonly AurenDbContext _context;
		private readonly DbSet<TEntity> _dbSet;

		public Repository(AurenDbContext context)
		{
			_context = context;
			_dbSet = _context.Set<TEntity>();
		}

		public async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct)
		{
			await _dbSet.AddAsync(entity, ct);
			await _context.SaveChangesAsync(ct);

			return entity;
        }

		public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct)
		{
			var entity = await GetByIdAsync(id, userId, ct);

			if (entity is null)
				return false;

			_dbSet.Remove(entity);
			await _context.SaveChangesAsync(ct);

			return true;
        }

		public async Task<TEntity?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct)
		{
			return await _dbSet
				.AsNoTracking()
				.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, ct);
		}

		public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct)
		{
			_dbSet.Update(entity);
			await _context.SaveChangesAsync(ct);

			return entity;
        }
	}
}
