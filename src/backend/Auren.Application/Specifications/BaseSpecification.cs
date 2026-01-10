using Auren.Application.Interfaces.Specification;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications
{
	public abstract class BaseSpecification<TEntity> : ISpecification<TEntity>
	{
        public abstract Expression<Func<TEntity, bool>> ToExpression();

        public ISpecification<TEntity> And(ISpecification<TEntity> other)
        {
            return new AndSpecification<TEntity>(this, other);
        }

        public ISpecification<TEntity> Or(ISpecification<TEntity> other)
        {
            return new OrSpecification<TEntity>(this, other);
        }
    }

    public class AndSpecification<TEntity> : BaseSpecification<TEntity>
    {
        private readonly ISpecification<TEntity> _left;
        private readonly ISpecification<TEntity> _right;

        public AndSpecification(ISpecification<TEntity> left, ISpecification<TEntity> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<TEntity, bool>> ToExpression()
        {
            var leftExpr = _left.ToExpression();
            var rightExpr = _right.ToExpression();

            var param = Expression.Parameter(typeof(TEntity), "x");
            var body = Expression.AndAlso(
                Expression.Invoke(leftExpr, param),
                Expression.Invoke(rightExpr, param)
            );

            return Expression.Lambda<Func<TEntity, bool>>(body, param);
        }
    }

    public class OrSpecification<TEntity> : BaseSpecification<TEntity>
    {
        private readonly ISpecification<TEntity> _left;
        private readonly ISpecification<TEntity> _right;

        public OrSpecification(ISpecification<TEntity> left, ISpecification<TEntity> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<TEntity, bool>> ToExpression()
        {
            var leftExpr = _left.ToExpression();
            var rightExpr = _right.ToExpression();

            var param = Expression.Parameter(typeof(TEntity), "x");
            var body = Expression.OrElse(
                Expression.Invoke(leftExpr, param),
                Expression.Invoke(rightExpr, param)
            );

            return Expression.Lambda<Func<TEntity, bool>>(body, param);
        }
    }
}
