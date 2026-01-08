using Auren.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Goals
{
	public class GoalTargetDateRangeSpecification(DateTime minDate, DateTime maxDate) : BaseSpecification<Goal>
	{
		public override Expression<Func<Goal, bool>> ToExpression()
		{
			return g => g.TargetDate >= minDate && g.TargetDate <= maxDate;
        }
	}
}
