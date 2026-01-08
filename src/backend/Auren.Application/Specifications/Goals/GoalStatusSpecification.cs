using Auren.Domain.Entities;
using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Goals
{
	public class GoalStatusSpecification(GoalStatus goalStatus) : BaseSpecification<Goal>
	{
		public override Expression<Func<Goal, bool>> ToExpression()
		{
			return g => g.Status == goalStatus;
		}
	}
}
