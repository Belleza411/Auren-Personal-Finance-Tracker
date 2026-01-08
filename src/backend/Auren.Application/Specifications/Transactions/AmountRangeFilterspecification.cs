using Auren.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Transactions
{
	public class AmountRangeFilterspecification(decimal minAmount, decimal maxAmount) : BaseSpecification<Transaction>
	{
		public override Expression<Func<Transaction, bool>> ToExpression()
		{
			return t => t.Amount >= minAmount && t.Amount <= maxAmount;
        }
	}
}
