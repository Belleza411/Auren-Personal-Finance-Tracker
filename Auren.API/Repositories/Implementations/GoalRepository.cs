using Auren.API.Data;
using Auren.API.DTOs.Requests;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auren.API.Repositories.Implementations
{
	public class GoalRepository : IGoalRepository
	{
		public ILogger<GoalRepository> _logger { get; }
        private readonly AurenDbContext _dbContext;

		public GoalRepository(ILogger<GoalRepository> logger, AurenDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		public async Task<IEnumerable<Goal>> GetGoalsAsync(Guid userId, CancellationToken cancellationToken)
		{
			try
			{
				var goal = await _dbContext.Goals
					.Where(g => g.UserId == userId)
					.ToListAsync(cancellationToken);

				if(!goal.Any())
				{
					_logger.LogWarning("No goals found for user {UserId}", userId);
                }

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
			if(goalDto == null)
			{
				_logger.LogWarning("GoalDto is null for user {UserId}", userId);
				throw new ArgumentException("Goal data is required");
            }

			try
			{
				var goal = new Goal {
					GoalId = Guid.NewGuid(),
					UserId = userId,
					Name = goalDto.Name,
					Description = goalDto.Description,
					Budget = goalDto.Budget,
					TargetDate = goalDto.TargetDate,
					Status = goalDto.Status,
					CreatedAt = DateTime.UtcNow
				};

				await _dbContext.Goals.AddAsync(goal, cancellationToken);
				await _dbContext.SaveChangesAsync(cancellationToken);
				_logger.LogInformation("{Goal} with {GoalId} was created succesfully for {UserId}", goal, goal.GoalId, userId);

				return goal;
            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to create goal for user {UserId}", userId);
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

			goal.Spent += amount;

			await _dbContext.SaveChangesAsync(cancellationToken);

			_logger.LogInformation("Added {Amount} to goal with id of {GoalId} for user {UserId}", amount, goalId, userId);
			return goal;
        }
	}
}
