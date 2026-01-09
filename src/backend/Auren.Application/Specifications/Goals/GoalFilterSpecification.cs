using Auren.Application.DTOs.Filters;
using Auren.Application.Interfaces.Specification;
using Auren.Application.Specifications.Common;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Goals
{
	public class GoalFilterSpecification : BaseSpecification<Goal>
    {
		private readonly Guid _userId;
        private readonly GoalFilter _goalFilter;

		public GoalFilterSpecification(Guid userId, GoalFilter goalFilter)
		{
			_userId = userId;
			_goalFilter = goalFilter ?? new GoalFilter();
		}

		public override Expression<Func<Goal, bool>> ToExpression()
		{
			ISpecification<Goal> spec = new UserSpecification<Goal>(_userId);

			if (!HasActiveFilters(_goalFilter))
				return spec.ToExpression();

			spec = ApplyFilters(spec);

            return spec.ToExpression();
		}

		private static bool HasActiveFilters(GoalFilter filter)
		{
			if (filter == null) return false;

			return !string.IsNullOrWhiteSpace(filter.SearchTerm) ||
				   filter.GoalStatus.HasValue ||
				   filter.MinBudget > 0 ||
				   filter.MaxBudget > 0 ||
				   filter.MinCompletionPercentage.HasValue ||
				   filter.MaxCompletionPercentage.HasValue ||
				   filter.TargetFrom.HasValue ||
				   filter.TargetTo.HasValue;
        }

		private ISpecification<Goal> ApplyFilters(ISpecification<Goal> spec)
		{
			if (_goalFilter.GoalStatus.HasValue)
			{
				spec = spec.And(new GoalStatusSpecification(_goalFilter.GoalStatus.Value));
            }

			if(_goalFilter.MinBudget.HasValue && _goalFilter.MaxBudget.HasValue)
			{
				spec = spec.And(new GoalBudgetRangeSpecification(_goalFilter.MinBudget.Value, _goalFilter.MaxBudget.Value));
            }

			if(_goalFilter.MinCompletionPercentage.HasValue && _goalFilter.MaxCompletionPercentage.HasValue)
			{
				spec = spec.And(new GoalCompletionPercentageRangeSpecification(_goalFilter.MinCompletionPercentage.Value, _goalFilter.MaxCompletionPercentage.Value));
			}

			if(_goalFilter.TargetFrom.HasValue && _goalFilter.TargetTo.HasValue)
			{
				spec = spec.And(new GoalTargetDateRangeSpecification(_goalFilter.TargetFrom.Value, _goalFilter.TargetTo.Value));
			}

            if (!string.IsNullOrEmpty(_goalFilter.SearchTerm))
            {
				spec = spec.And(BuildSearchSpecification(_goalFilter.SearchTerm.Trim()));
            }

            return spec;
		}

		private static ISpecification<Goal> BuildSearchSpecification(string searchTerm)
		{
			ISpecification<Goal> spec = new NameFilterSpecification<Goal>(searchTerm);

            spec = new GoalDescriptionSearchSpecification(searchTerm);
			spec = TryAddGoalStatus(spec, searchTerm);

			return spec;
        }

		private static ISpecification<Goal> TryAddGoalStatus(ISpecification<Goal> spec, string searchTerm)
		{
			if (Enum.TryParse<GoalStatus>(searchTerm, true, out var status))
			{
				spec.Or(new GoalStatusSpecification(status));
			}

			return spec;
        }
	}
}
