using Auren.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Goals
{
	public class GoalCompletionPercentageRangeSpecification(int minCompletion, int maxCompletion) : BaseSpecification<Goal>
	{
		public override Expression<Func<Goal, bool>> ToExpression()
		{
			return g => g.CompletionPercentage >= minCompletion && g.CompletionPercentage <= maxCompletion;
		}
	}
}
