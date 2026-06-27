using Auren.Domain.Entities;
using System.Linq.Expressions;

namespace Auren.Application.Common.Specifications.Categories
{
    public class CategoriesFilterSpec(IEnumerable<Guid> categoriesIds) : BaseSpecification<Category>
    {
        public override Expression<Func<Category, bool>> ToExpression()
        {
            return c => categoriesIds.Contains(c.Id);
        }
    }
}
