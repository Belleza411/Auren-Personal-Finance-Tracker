

using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Responses.Goal;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Specifications.Goals;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auren.Infrastructure.Repositories
{
	public class GoalRepository : Repository<Goal>, IGoalRepository
	{
		private readonly AurenDbContext _dbContext;

		public GoalRepository(AurenDbContext dbContext) : base(dbContext)
        {
			_dbContext = dbContext;
		}

		public async Task<IEnumerable<Goal>> GetGoalsAsync(
			Guid userId, GoalFilter filter,
			int pageSize = 5, int pageNumber = 1,
			CancellationToken cancellationToken = default)
		{
			var skip = (pageNumber - 1) * pageSize;

			var spec = new GoalFilterSpecification(userId, filter);
			var query = _dbContext.Goals.Where(spec.ToExpression());

			var goal = await query
				.OrderByDescending(g => g.Spent)
				.ThenByDescending(g => g.CreatedAt)
				.Skip(skip)
				.Take(pageSize)
				.AsNoTracking()
				.ToListAsync(cancellationToken);

            return goal;
        }
		public async Task<GoalsSummaryResponse> GetGoalsSummaryAsync(Guid userId, CancellationToken cancellationToken)
		{
            var activeStatuses = new[]
			{
				GoalStatus.OnTrack,
				GoalStatus.NotStarted,
				GoalStatus.BehindSchedule
			};

            var goals = _dbContext.Goals.Where(g => g.UserId == userId);

            var goalsSummary = new
            {
                TotalGoals = await goals.CountAsync(cancellationToken),
                GoalsCompleted = await goals.CountAsync(x => x.Status == GoalStatus.Completed, cancellationToken),
                ActiveGoals = await goals.CountAsync(x => activeStatuses.Contains(x.Status), cancellationToken)
            };

            return new GoalsSummaryResponse(
				TotalGoals: goalsSummary.TotalGoals,
				GoalsCompleted: goalsSummary.GoalsCompleted,
				ActiveGoals: goalsSummary.ActiveGoals
			);
        }
    }
}
