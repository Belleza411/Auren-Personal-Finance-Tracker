

using Auren.Application.Common.Result;
using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses.Goal;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using FluentValidation;

namespace Auren.Application.Services
{
	public class GoalService : IGoalService
	{
		private readonly IValidator<GoalDto> _validator;
		private readonly IGoalRepository _goalRepository;
		private readonly ITransactionRepository _transactionRepository;
		private readonly ICategoryRepository _categoryRepository;
		public GoalService(IValidator<GoalDto> validator, IGoalRepository goalRepository, ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
		{
			_validator = validator;
			_goalRepository = goalRepository;
			_transactionRepository = transactionRepository;
			_categoryRepository = categoryRepository;
		}

		public async Task<Result<IEnumerable<Goal>>> GetGoals(Guid userId, GoalFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default)
        {
			var goals = await _goalRepository.GetGoalsAsync(userId, filter, pageSize, pageNumber, cancellationToken);

			foreach (var goal in goals)
			{
                goal.CompletionPercentage = GetCompletionPercentage(goal.Spent ?? 0, goal.Budget);
                goal.TimeRemaining = GetTimeRemaining(DateTime.UtcNow, goal.TargetDate);
            }

			return Result.Success<IEnumerable<Goal>>(goals);
        }
			
		public async Task<Result<Goal>> GetGoalById(Guid goalId, Guid userId, CancellationToken cancellationToken)
		{
			var goal = await _goalRepository.GetGoalByIdAsync(goalId, userId, cancellationToken);

			if (goal == null)
				return Result.Failure<Goal>(Error.NotFound($"Goal with id of {goalId} is not found."));

            goal.CompletionPercentage = GetCompletionPercentage(goal.Spent ?? 0, goal.Budget);
            goal.TimeRemaining = GetTimeRemaining(DateTime.UtcNow, goal.TargetDate);

            return Result.Success<Goal>(goal);
		}

		public async Task<Result<Goal>> CreateGoal(GoalDto goalDto, Guid userId, CancellationToken cancellationToken)
		{
			if (goalDto == null)
				return Result.Failure<Goal>(Error.InvalidInput("All fields are required."));

			var validationResult = await _validator.ValidateAsync(goalDto, cancellationToken);
			if (!validationResult.IsValid)
			{
				var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
				return Result.Failure<Goal>(Error.ValidationFailed(errors));
			}

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

			var createdGoal = await _goalRepository.CreateGoalAsync(goal, userId, cancellationToken);

			return createdGoal == null
				? Result.Failure<Goal>(Error.CreateFailed("Failed to create goal."))
				: Result.Success(createdGoal);
		}

		public async Task<Result<Goal>> UpdateGoal(Guid goalId, Guid userId, GoalDto goalDto, CancellationToken cancellationToken)
		{
			if (goalDto == null)
				return Result.Failure<Goal>(Error.InvalidInput("All fields are required."));

			var validationResult = await _validator.ValidateAsync(goalDto, cancellationToken);
			if (!validationResult.IsValid)
			{
				var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
				return Result.Failure<Goal>(Error.ValidationFailed(errors));
			}

			var existingGoal = await _goalRepository.GetGoalByIdAsync(goalId, userId, cancellationToken);

			if (existingGoal == null)
				return Result.Failure<Goal>(Error.NotFound($"Goal with id of {goalId} is not found."));

			existingGoal.GoalId = goalId;
			existingGoal.UserId = userId;
			existingGoal.Name = goalDto.Name;
			existingGoal.Description = goalDto.Description;
			existingGoal.Spent = goalDto.Spent;
			existingGoal.Budget = goalDto.Budget;
			existingGoal.TargetDate = goalDto.TargetDate;
			existingGoal.Status = goalDto.Status;
			existingGoal.CompletionPercentage = GetCompletionPercentage(existingGoal.Spent ?? 0, existingGoal.Budget);
			existingGoal.TimeRemaining = GetTimeRemaining(DateTime.UtcNow, existingGoal.TargetDate);

			var updatedGoal = await _goalRepository.UpdateGoalAsync(existingGoal, cancellationToken);

			return updatedGoal == null
				? Result.Failure<Goal>(Error.UpdateFailed("Failed to update goal."))
				: Result.Success(updatedGoal);
		}

		public async Task<Result<bool>> DeleteGoal(Guid goalId, Guid userId, CancellationToken cancellationToken)
		{
			var existingGoal = await _goalRepository.GetGoalByIdAsync(goalId, userId, cancellationToken);
			if (existingGoal == null)
				return Result.Failure<bool>(Error.NotFound($"Goal with id of {goalId} not found. "));

			var deletedGoal = await _goalRepository.DeleteGoalAsync(goalId, userId, cancellationToken);

			return deletedGoal
				? Result.Success(true)
				: Result.Failure<bool>(Error.DeleteFailed("Failed to delete goal"));
		}

		public async Task<Result<Goal>> AddMoneyToGoal(Guid goalId, Guid userId, decimal amount, CancellationToken cancellationToken)
		{
			if (amount < 0)
				return Result.Failure<Goal>(Error.GoalError.AmountMustBePositive("Amount must be a positive value."));

			var currentBalance = await _transactionRepository.GetBalanceAsync(userId, DateTime.MinValue, DateTime.Today, cancellationToken);

			if (currentBalance < amount)
				return Result.Failure<Goal>(Error.NotEnoughBalance($"{amount} is not enough balance. "));

			var existingGoal = await _goalRepository.GetGoalByIdAsync(goalId, userId, cancellationToken);

			if (existingGoal == null)
				return Result.Failure<Goal>(Error.NotFound($"Goal with id of {goalId} is not found."));

			var goalCategory = new CategoryDto("Goal Transfer", TransactionType.Expense);

			var existingCategory = await _categoryRepository.GetCategoryByNameAsync(userId, goalCategory, cancellationToken);

			if (existingCategory == null)
			{
				var newCategory = new Category
				{
					CategoryId = Guid.NewGuid(),
					UserId = userId,
					Name = goalCategory.Name,
					TransactionType = goalCategory.TransactionType,
					CreatedAt = DateTime.UtcNow
				};

				existingCategory = await _categoryRepository.CreateCategoryAsync(newCategory, cancellationToken);
			}

			var newGoalTransaction = new Transaction
			{
				TransactionId = Guid.NewGuid(),
				UserId = userId,
				CategoryId = existingCategory.CategoryId,
				Amount = amount,
				Name = $"Transfer to goal: {existingGoal.Name}",
				PaymentType = PaymentType.Other,
				TransactionType = TransactionType.Expense,
				TransactionDate = DateTime.UtcNow,
				CreatedAt = DateTime.UtcNow
			};

			var transactionResult = await _transactionRepository.CreateTransactionAsync(newGoalTransaction, userId, cancellationToken);

			if (transactionResult == null)
				return Result.Failure<Goal>(Error.CreateFailed("Failed to create transaction for adding money to goal."));

			existingGoal.Spent = (existingGoal.Spent ?? 0) + amount;
			existingGoal.CompletionPercentage = GetCompletionPercentage(existingGoal.Spent ?? 0, existingGoal.Budget);

			var goalWithAddedMoney = await _goalRepository.UpdateGoalAsync(existingGoal, cancellationToken);

			return goalWithAddedMoney == null
				? Result.Failure<Goal>(Error.UpdateFailed("Failed to add money to goal."))
				: Result.Success(goalWithAddedMoney);
		}

		public async Task<Result<GoalsSummaryResponse>> GetGoalsSummary(Guid userId, CancellationToken cancellationToken)
			=> Result.Success(await _goalRepository.GetGoalsSummaryAsync(userId, cancellationToken));

		private static int GetCompletionPercentage(decimal currentAmount, decimal targetAmount)
		{
			if (targetAmount <= 0m)
				return 0;

			decimal percentage = (currentAmount / targetAmount) * 100m;

			int rounded = (int)Math.Round(percentage, MidpointRounding.AwayFromZero);
			if (rounded < 0) return 0;
			if (rounded > 100) return 100;
			return rounded;
		}

		private static string GetTimeRemaining(DateTime startDate, DateTime targetDate)
		{
			if (targetDate <= startDate)
				return "The target date has already passed.";

			TimeSpan difference = targetDate - startDate;

			int totalDays = (int)difference.TotalDays;
			int months = totalDays / 30;

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
	}
}
