using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Interfaces.Specification
{
	public interface ISpecification<TEntity>
	{
		Expression<Func<TEntity, bool>> ToExpression();
        ISpecification<TEntity> And(ISpecification<TEntity> other);
        ISpecification<TEntity> Or(ISpecification<TEntity   > other);
    }
}
