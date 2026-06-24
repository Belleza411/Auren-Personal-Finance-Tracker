using Auren.Application.Common.Specifications;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Common.Specifications.Transactions
{
	public class PaymentTypeFilterSpecification(PaymentType type) : BaseSpecification<Transaction>
    {
		public override Expression<Func<Transaction, bool>> ToExpression()
		{
			return t => t.PaymentType == type;
        }
    }
}
