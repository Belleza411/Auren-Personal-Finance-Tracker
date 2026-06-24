using Auren.Domain.Common;
using Auren.Domain.Enums;
using System.Linq.Expressions;

namespace Auren.Application.Common.Specifications.Common
{
	public class TransactionTypeFilterSpecification<TEntity>(TransactionType type) : BaseSpecification<TEntity> where TEntity : IHasTransactionType
    {
		public override Expression<Func<TEntity, bool>> ToExpression()
		{
			return e => e.TransactionType == type;
		}
	}
}
