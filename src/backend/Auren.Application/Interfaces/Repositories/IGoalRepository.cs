using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Responses.Goal;
using Auren.Domain.Entities;

namespace Auren.Application.Interfaces.Repositories
{
	public interface IGoalRepository : IRepository<Goal>
	{
        Task<IEnumerable<Goal>> GetGoalsAsync(Guid userId, GoalFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
        Task<GoalsSummaryResponse> GetGoalsSummaryAsync(Guid userId, CancellationToken cancellationToken);
    }
}
