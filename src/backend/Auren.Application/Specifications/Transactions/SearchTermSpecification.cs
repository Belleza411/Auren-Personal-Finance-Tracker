using Auren.Domain.Entities;
using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Transactions
{
	public class SearchTermSpecification
	{
		public class NameSearchSpecification(string searchTerm) : BaseSpecification<Transaction>
		{
			public override Expression<Func<Transaction, bool>> ToExpression()
			{
				return t => t.Name.Contains(searchTerm);
			}
		}

		public class AmountSearchSpecification(decimal amount) : BaseSpecification<Transaction>
		{
			public override Expression<Func<Transaction, bool>> ToExpression()
			{

				return t => t.Amount == amount;
            }
		}

		public class DateSearchSpecification(DateTime date) : BaseSpecification<Transaction>
		{
			public override Expression<Func<Transaction, bool>> ToExpression()
			{

				return t => t.TransactionDate == date;
            }
		}
	}
}
