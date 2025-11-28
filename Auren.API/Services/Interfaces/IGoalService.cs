using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Helpers.Result;
using Auren.API.Models.Domain;

namespace Auren.API.Services.Interfaces
{
	public interface IGoalService
	{
        Task<Result<IEnumerable<Goal>>> GetGoals(Guid userId, GoalFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
        Task<Result<Goal>> GetGoalById(Guid goalId, Guid userId, CancellationToken cancellationToken);
        Task<Result<Goal>> CreateGoal(GoalDto goalDto, Guid userId, CancellationToken cancellationToken);
        Task<Result<Goal>> UpdateGoal(Guid goalId, Guid userId, GoalDto goalDto, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteGoal(Guid goalId, Guid userId, CancellationToken cancellationToken);
        Task<Result<Goal>> AddMoneyToGoal(Guid goalId, Guid userId, decimal amount, CancellationToken cancellationToken);
        Task<Result<GoalsSummaryResponse>> GetGoalsSummary(Guid userId, CancellationToken cancellationToken);
    }
}
