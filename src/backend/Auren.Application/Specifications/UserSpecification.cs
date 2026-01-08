using Auren.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications
{
	public class UserSpecification<T>(Guid userId) : BaseSpecification<T> where T : IEntity
	{
		public override Expression<Func<T, bool>> ToExpression()
		{
			return e => e.UserId == userId;
		}
	}
}
