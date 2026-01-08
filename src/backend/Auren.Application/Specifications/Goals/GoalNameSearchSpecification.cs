using Auren.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Goals
{
	public class GoalNameSearchSpecification(string searchTerm) : BaseSpecification<Goal>
	{
		public override Expression<Func<Goal, bool>> ToExpression()
		{
			return g => g.Name.Contains(searchTerm);
		}
	}
}
