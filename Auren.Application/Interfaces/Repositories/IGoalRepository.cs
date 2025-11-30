using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Responses;
using Auren.Domain.Entities;

namespace Auren.Application.Interfaces.Repositories
{
	public interface IGoalRepository
	{
        Task<IEnumerable<Goal>> GetGoalsAsync(Guid userId, GoalFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
        Task<Goal?> GetGoalByIdAsync(Guid goalId, Guid userId, CancellationToken cancellationToken);
        Task<Goal> CreateGoalAsync(Goal goal, Guid userId, CancellationToken cancellationToken);
        Task<Goal?> UpdateGoalAsync(Goal goal, CancellationToken cancellationToken);
        Task<bool> DeleteGoalAsync(Guid goalId, Guid userId, CancellationToken cancellationToken);
        Task<GoalsSummaryResponse> GetGoalsSummaryAsync(Guid userId, CancellationToken cancellationToken);
    }
}
