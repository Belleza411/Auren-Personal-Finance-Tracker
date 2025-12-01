using Auren.Application.Common.Result;
using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses.Goal;
using Auren.Domain.Entities;

namespace Auren.Application.Interfaces.Services
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
