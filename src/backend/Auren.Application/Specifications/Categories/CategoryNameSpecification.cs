using Auren.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Categories
{
	public class CategoryNameSpecification(string category) : BaseSpecification<Category>
	{
		public override Expression<Func<Category, bool>> ToExpression()
		{
			return c => c.Name.Contains(category);
        }
	}
}
