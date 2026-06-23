using Auren.Application.Common.Specifications;
using Auren.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Common.Specifications.Transactions
{
	public class CategoriesFilterSpec(IEnumerable<Guid> categoriesIds) : BaseSpecification<Transaction>
	{
		public override Expression<Func<Transaction, bool>> ToExpression()
		{
			return t => categoriesIds.Contains(t.CategoryId);
		}
	}
}
