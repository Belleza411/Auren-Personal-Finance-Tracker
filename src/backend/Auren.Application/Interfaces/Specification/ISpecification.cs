using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Interfaces.Specification
{
	public interface ISpecification<T>
	{
		Expression<Func<T, bool>> ToExpression();
        ISpecification<T> And(ISpecification<T> other);
        ISpecification<T> Or(ISpecification<T> other);
    }
}
