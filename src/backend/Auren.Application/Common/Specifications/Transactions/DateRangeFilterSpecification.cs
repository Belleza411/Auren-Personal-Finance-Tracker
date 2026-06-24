using Auren.Application.Common.Specifications;
using Auren.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Common.Specifications.Transactions
{
	public class DateRangeFilterSpecification(DateTime startDate, DateTime endDate) : BaseSpecification<Transaction>
	{
		public override Expression<Func<Transaction, bool>> ToExpression()
		{
			return t => t.TransactionDate >= startDate && t.TransactionDate <= endDate;
		}
	}
}
