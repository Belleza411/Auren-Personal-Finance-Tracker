

using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Responses.Goal;
using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auren.Infrastructure.Repositories
{
	public class GoalRepository(AurenDbContext dbContext) : IGoalRepository
	{
		public async Task<IEnumerable<Goal>> GetGoalsAsync(
			Guid userId, GoalFilter filter,
			int pageSize = 5, int pageNumber = 1,
			CancellationToken cancellationToken = default)
		{
			var skip = (pageNumber - 1) * pageSize;

			var query = dbContext.Goals
				.Where(g => g.UserId == userId);

            if(HasActiveFilters(filter))
				query = ApplyFilters(query, filter);
            
			var goal = await query
				.Where(g => g.UserId == userId)
				.OrderByDescending(g => g.Spent)
				.ThenByDescending(g => g.CreatedAt)
				.Skip(skip)
				.Take(pageSize)
				.AsNoTracking()
				.ToListAsync(cancellationToken);

            return goal;
        }

		public async Task<Goal?> GetGoalByIdAsync(Guid goalId, Guid userId, CancellationToken cancellationToken)
			=> await dbContext.Goals
				.FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId, cancellationToken);

        public async Task<Goal> CreateGoalAsync(Goal goal, Guid userId, CancellationToken cancellationToken)
        {
            await dbContext.Goals.AddAsync(goal, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return goal;
        }

        public async Task<Goal?> UpdateGoalAsync(Goal goal, CancellationToken cancellationToken)
		{
			dbContext.Goals.Update(goal);
            await dbContext.SaveChangesAsync(cancellationToken);

			return goal;     
        }

		public async Task<bool> DeleteGoalAsync(Guid goalId, Guid userId, CancellationToken cancellationToken)
		{
			var goal = await dbContext.Goals
				.FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId, cancellationToken);

			if (goal == null) return false;

			dbContext.Goals.Remove(goal);
			await dbContext.SaveChangesAsync(cancellationToken);

			return true;
		}

		private static IQueryable<Goal> ApplyFilters(IQueryable<Goal> query, GoalFilter filter)
		{
			if (filter == null)
				return query;
			
			if (filter.IsCompleted.HasValue)
				query = query.Where(g => g.Status == GoalStatus.Completed);
            	
			if (filter.IsOnTrack.HasValue)
				query = query.Where(g => g.Status == GoalStatus.OnTrack);
			
			if (filter.IsOnHold.HasValue)
				query = query.Where(g => g.Status == GoalStatus.OnHold);
			
			if (filter.IsNotStarted.HasValue)
				query = query.Where(g => g.Status == GoalStatus.NotStarted);
			
			if (filter.IsBehindSchedule.HasValue)
				query = query.Where(g => g.Status == GoalStatus.BehindSchedule);
			
			if (filter.IsCancelled.HasValue)
				query = query.Where(g => g.Status == GoalStatus.Cancelled);
			
			return query;
        }

		private static bool HasActiveFilters(GoalFilter filter)
		{
			if(filter == null)
				return false;

			return filter.IsCompleted.HasValue ||
				filter.IsOnTrack.HasValue ||
				filter.IsOnHold.HasValue ||
				filter.IsNotStarted.HasValue ||
				filter.IsBehindSchedule.HasValue ||
				filter.IsCancelled.HasValue;
        }

		public async Task<GoalsSummaryResponse> GetGoalsSummaryAsync(Guid userId, CancellationToken cancellationToken)
		{
            var activeStatuses = new[]
			{
				GoalStatus.OnTrack,
				GoalStatus.NotStarted,
				GoalStatus.BehindSchedule
			};

            var goals = dbContext.Goals.Where(g => g.UserId == userId);

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
