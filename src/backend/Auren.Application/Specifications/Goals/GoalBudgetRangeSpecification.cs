using Auren.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Goals
{
	public class GoalBudgetRangeSpecification(decimal minBudget, decimal maxBudget) : BaseSpecification<Goal>
	{
		public override Expression<Func<Goal, bool>> ToExpression()
		{
			return g => g.Budget >= minBudget && g.Budget <= maxBudget;
        }
	}
}
