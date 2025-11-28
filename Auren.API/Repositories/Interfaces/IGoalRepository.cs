using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Models.Domain;
using Auren.API.Models.Enums;

namespace Auren.API.Repositories.Interfaces
{
	public interface IGoalRepository
	{
        Task<IEnumerable<Goal>> GetGoalsAsync(Guid userId, GoalFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
        Task<Goal?> GetGoalByIdAsync(Guid goalId, Guid userId, CancellationToken cancellationToken);
        Task<Goal> CreateGoalAsync(Goal goal, Guid userId, CancellationToken cancellationToken);
        Task<Goal?> UpdateGoalAsync(Guid goalId, Guid userId, Goal goal, CancellationToken cancellationToken);
        Task<bool> DeleteGoalAsync(Guid goalId, Guid userId, CancellationToken cancellationToken);
        Task<Goal?> AddMoneyToGoalAsync(Goal goal, Guid userId, decimal amount, CancellationToken cancellationToken);
        Task<GoalsSummaryResponse> GetGoalsSummaryAsync(Guid userId, CancellationToken cancellationToken);
    }
}
