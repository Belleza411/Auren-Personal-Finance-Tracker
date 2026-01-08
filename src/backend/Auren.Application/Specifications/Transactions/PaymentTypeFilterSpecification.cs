using Auren.Domain.Entities;
using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Transactions
{
	public class PaymentTypeFilterSpecification(PaymentType type) : BaseSpecification<Transaction>
    {
		public override Expression<Func<Transaction, bool>> ToExpression()
		{
			return t => t.PaymentType == type;
        }
    }
}
