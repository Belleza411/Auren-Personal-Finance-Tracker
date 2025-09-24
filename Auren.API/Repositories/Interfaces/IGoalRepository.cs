using Auren.API.DTOs.Requests;
using Auren.API.Models.Domain;

namespace Auren.API.Repositories.Interfaces
{
	public interface IGoalRepository
	{
        Task<IEnumerable<Goal>> GetGoalsAsync(Guid userId, CancellationToken cancellationToken, int? pageSize, int? pageNumber);
        Task<Goal?> GetGoalByIdAsync(Guid goalId, Guid userId, CancellationToken cancellationToken);
        Task<Goal> CreateGoalAsync(GoalDto goalDto, Guid userId, CancellationToken cancellationToken);
        Task<Goal?> UpdateGoalAsync(Guid goalId, Guid userId, GoalDto goalDto, CancellationToken cancellationToken);
        Task<bool> DeleteGoalAsync(Guid goalId, Guid userId, CancellationToken cancellationToken);
        Task<Goal?> AddMoneyToGoalAsync(Guid goalId, Guid userId, decimal amount, CancellationToken cancellationToken);
    }
}
