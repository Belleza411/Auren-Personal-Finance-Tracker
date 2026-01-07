using Auren.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auren.Application.Interfaces.Repositories
{
	public interface IRepository<T> where T : class
	{
		Task<T?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct);
		Task<T> AddAsync(T entity, CancellationToken ct);
		Task<T> UpdateAsync(T entity, CancellationToken ct);
		Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct);
	}
}
