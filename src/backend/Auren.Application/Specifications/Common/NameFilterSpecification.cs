using Auren.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Common
{
	public class NameFilterSpecification<T>(string name) : BaseSpecification<T> where T : IHasName
	{
		public override Expression<Func<T, bool>> ToExpression()
		{
			return e => e.Name == name;
		}
	}
}
