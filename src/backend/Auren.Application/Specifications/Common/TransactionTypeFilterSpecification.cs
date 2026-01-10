using Auren.Application.DTOs.Filters;
using Auren.Domain.Common;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Common
{
	public class TransactionTypeFilterSpecification<TEntity>(TransactionType type) : BaseSpecification<TEntity> where TEntity : IHasTransactionType
    {
		public override Expression<Func<TEntity, bool>> ToExpression()
		{
			return e => e.TransactionType == type;
		}
	}
}
