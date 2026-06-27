using Auren.Domain.Common;
using System.Linq.Expressions;

namespace Auren.Application.Common.Specifications
{
	public class UserSpecification<TEntity>(Guid userId) : BaseSpecification<TEntity> where TEntity : IEntity
	{
		public override Expression<Func<TEntity, bool>> ToExpression()
		{
			return e => e.UserId == userId;
		}
	}
}
