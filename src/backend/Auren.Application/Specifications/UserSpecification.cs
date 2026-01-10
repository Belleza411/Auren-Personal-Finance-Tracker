using Auren.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications
{
	public class UserSpecification<TEntity>(Guid userId) : BaseSpecification<TEntity> where TEntity : IEntity
	{
		public override Expression<Func<TEntity, bool>> ToExpression()
		{
			return e => e.UserId == userId;
		}
	}
}
