using Auren.Application.Common.Specifications;
using Auren.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Common.Specifications.Common
{
	public class NameFilterSpecification<TEntity>(string name) : BaseSpecification<TEntity> where TEntity : IHasName
	{
		public override Expression<Func<TEntity, bool>> ToExpression()
		{
			return e => e.Name.Contains(name.Trim());
		}
	}
}
