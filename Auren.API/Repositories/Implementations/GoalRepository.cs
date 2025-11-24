using Auren.API.Data;
using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Models.Domain;
using Auren.API.Models.Enums;
using Auren.API.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Auren.API.Repositories.Implementations
{
	public class GoalRepository : IGoalRepository
	{
		public ILogger<GoalRepository> _logger { get; }
        private readonly AurenDbContext _dbContext;
        private readonly string _connectionString;

        public GoalRepository(ILogger<GoalRepository> logger, AurenDbContext dbContext, IConfiguration configuration)
		{
			_logger = logger;
			_dbContext = dbContext;
            _connectionString = configuration.GetConnectionString("AurenDbConnection") ?? throw new ArgumentNullException("Connection string not found.");
        }

		public async Task<IEnumerable<Goal>> GetGoalsAsync(Guid userId, CancellationToken cancellationToken, GoalFilter filter, int? pageSize, int? pageNumber)
		{
			try
			{
				var skip = ((pageNumber ?? 1) - 1) * (pageSize ?? 5);

				var query = _dbContext.Goals
					.Where(g => g.UserId == userId);

                if(HasActiveFilters(filter))
				{
					query = ApplyFilters(query, filter);
                }

				var goal = await query
					.Where(g => g.UserId == userId)
					.OrderByDescending(g => g.Spent)
					.ThenByDescending(g => g.CreatedAt)
					.Skip(skip)
					.Take(pageSize ?? 5)
					.AsNoTracking()
					.ToListAsync(cancellationToken);

				_logger.LogInformation("Retrieved {Count} goals for user {UserId}", goal.Count, userId);

                return goal;
            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve goals for user {UserId}", userId);
				throw;
            }
        }

		public async Task<Goal?> GetGoalByIdAsync(Guid goalId, Guid userId, CancellationToken cancellationToken)
		{
			try
			{
				var goal = await _dbContext.Goals
					.FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId, cancellationToken);

				if (goal == null)
				{
					_logger.LogWarning("Goal with id of {GoalId} not found for {UserId}", goalId, userId);
				}

				return goal;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve goal with id of {GoalId} for user {UserId}", goalId, userId);
				throw;
			}
		}

        public async Task<Goal> CreateGoalAsync(GoalDto goalDto, Guid userId, CancellationToken cancellationToken)
        {
            if (goalDto == null)
                throw new ArgumentException("Goal data is required");

            try
            {
                _logger.LogInformation("Spent: {Spent}, Budget: {Budget}", goalDto.Spent, goalDto.Budget);

                decimal spent = goalDto.Spent;
                decimal budget = goalDto.Budget;

                var goal = new Goal
                {
                    GoalId = Guid.NewGuid(),
                    UserId = userId,
                    Name = goalDto.Name,
                    Description = goalDto.Description,
                    Spent = spent,
                    Budget = budget,
                    TargetDate = goalDto.TargetDate,
                    Status = goalDto.Status,
                    CompletionPercentage = GetCompletionPercentage(spent, budget),
                    TimeRemaining = GetTimeRemaining(DateTime.UtcNow, goalDto.TargetDate),
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.Goals.AddAsync(goal, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return goal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create goal");
                throw;
            }
        }

        public async Task<Goal?> UpdateGoalAsync(Guid goalId, Guid userId, GoalDto goalDto, CancellationToken cancellationToken)
		{
			if(goalDto == null)
			{
				_logger.LogWarning("GoalDto is null for user {UserId}", userId);
				throw new ArgumentException("Goal data is required");
            }

			try
			{
				var goal = await _dbContext.Goals
					.FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId, cancellationToken);

				if(goal == null)
				{
					_logger.LogWarning("Goal with id of {GoalId} not found for {UserId}", goalId, userId);
					return null;
                }

				goal.GoalId = goalId;
				goal.UserId = userId;
                goal.Name = goalDto.Name;
				goal.Description = goalDto.Description;
				goal.Spent = goalDto.Spent;
                goal.Budget = goalDto.Budget;
				goal.TargetDate = goalDto.TargetDate;
				goal.Status = goalDto.Status;
                goal.CompletionPercentage = GetCompletionPercentage(goal.Spent ?? 0, goal.Budget);
                goal.TimeRemaining = GetTimeRemaining(DateTime.UtcNow, goal.TargetDate);


                await _dbContext.SaveChangesAsync(cancellationToken);
				_logger.LogInformation("{Goal} with {GoalId} was updated succesfully for {UserId}", goal, goal.GoalId, userId);

				return goal;
            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to update goal with id of {GoalId} for user {UserId}", goalId, userId);
				throw;
            }

        }

		public async Task<bool> DeleteGoalAsync(Guid goalId, Guid userId, CancellationToken cancellationToken)
		{
			try
			{
				var goal = await _dbContext.Goals
					.FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId, cancellationToken);

				if (goal == null)
				{
					_logger.LogWarning("Goal with id of {GoalId} not found for {UserId}", goalId, userId);
					return false;
				}

				_dbContext.Goals.Remove(goal);
				await _dbContext.SaveChangesAsync(cancellationToken);
				_logger.LogInformation("Goal with id of {GoalId} was deleted succesfully for {UserId}", goalId, userId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to delete goal with id of {GoalId} for user {UserId}", goalId, userId);
				return false;
			}
		}

		public async Task<Goal?> AddMoneyToGoalAsync(Guid goalId, Guid userId, decimal amount, CancellationToken cancellationToken)
		{
			if(amount < 0)
			{
				_logger.LogWarning("Amount must be greater than zero for user {UserId}", userId);
				throw new ArgumentException("Amount must be greater than zero");
            }

			var goal = await _dbContext.Goals
				.FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId, cancellationToken);

			if (goal == null)
			{
				_logger.LogWarning("Goal with id of {GoalId} not found for {UserId}", goalId, userId);
                return null;
			}

			goal.Spent = (goal.Spent ?? 0) + amount;
            goal.CompletionPercentage = GetCompletionPercentage(goal.Spent ?? 0, goal.Budget);

            await _dbContext.SaveChangesAsync(cancellationToken);

			_logger.LogInformation("Added {Amount} to goal with id of {GoalId} for user {UserId}", amount, goalId, userId);
			return goal;
        }

		private IQueryable<Goal> ApplyFilters(IQueryable<Goal> query, GoalFilter filter)
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

		private bool HasActiveFilters(GoalFilter filter)
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

		private int GetCompletionPercentage(decimal currentAmount, decimal targetAmount)
		{
            if (targetAmount <= 0m)
                return 0;

            decimal percentage = (currentAmount / targetAmount) * 100m;

            int rounded = (int)Math.Round(percentage, MidpointRounding.AwayFromZero);
            if (rounded < 0) return 0;
            if (rounded > 100) return 100;
            return rounded;
        }

		private string GetTimeRemaining(DateTime startDate, DateTime targetDate)
        {
            if (targetDate <= startDate)
                return "The target date has already passed.";

            TimeSpan difference = targetDate - startDate;

            int totalDays = (int)difference.TotalDays;
            int months = totalDays / 30;
            int days = totalDays % 30;

            if (months > 0)
            {
                return $"{months} month{(months > 1 ? "s" : "")}";
            }
            else if (totalDays >= 1)
            {
                return $"{totalDays} day{(totalDays > 1 ? "s" : "")} remaining.";
            }
            else
            {
                int hours = (int)difference.TotalHours;
                return $"{hours} hour{(hours > 1 ? "s" : "")} remaining.";
            }
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
                TotalGoals = await goals.CountAsync(),
                GoalsCompleted = await goals.CountAsync(x => x.Status == GoalStatus.Completed),
                ActiveGoals = await goals.CountAsync(x => activeStatuses.Contains(x.Status))
            };

            return new GoalsSummaryResponse(
				TotalGoals: goalsSummary.TotalGoals,
				GoalsCompleted: goalsSummary.GoalsCompleted,
				ActiveGoals: goalsSummary.ActiveGoals
			);
        }
    }
}
