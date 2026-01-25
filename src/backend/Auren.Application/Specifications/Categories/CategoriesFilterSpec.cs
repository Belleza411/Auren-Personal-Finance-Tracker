using Auren.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Categories
{
    public class CategoriesFilterSpec(IEnumerable<Guid> categoriesIds) : BaseSpecification<Category>
    {
        public override Expression<Func<Category, bool>> ToExpression()
        {
            return c => categoriesIds.Contains(c.Id);
        }
    }
}
