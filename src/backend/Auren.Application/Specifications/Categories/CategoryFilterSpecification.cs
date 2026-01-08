using Auren.Application.DTOs.Filters;
using Auren.Application.Interfaces.Specification;
using Auren.Application.Specifications.Common;
using Auren.Application.Specifications.Transactions;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Categories
{
	public class CategoryFilterSpecification : BaseSpecification<Category>
	{
		private readonly Guid _userId;
		private readonly CategoriesFilter _filter;
		public CategoryFilterSpecification(Guid userId, CategoriesFilter filter)
		{
			_userId = userId;
			_filter = filter ?? new CategoriesFilter();
        }
        public override Expression<Func<Category, bool>> ToExpression()
		{
			ISpecification<Category> spec = new UserSpecification<Category>(_userId);

			if(!HasActiveFilters(_filter)) 
				return spec.ToExpression();

			spec = ApplyFilters(spec);

			return spec.ToExpression();
        }

		private static bool HasActiveFilters(CategoriesFilter filter)
		{
			if (filter == null) return false;

			return !string.IsNullOrWhiteSpace(filter.SearchTerm) ||
				   filter.TransactionType.HasValue ||
				   !string.IsNullOrWhiteSpace(filter.Category);
        }

		private ISpecification<Category> ApplyFilters(ISpecification<Category> spec)
		{
            if (_filter.TransactionType.HasValue)
            {
                var typeSpec = new TransactionTypeFilterSpecification<Category>(_filter.TransactionType.Value);
                spec = new AndSpecification<Category>(spec, typeSpec);
            }
            
			if(!string.IsNullOrWhiteSpace(_filter.Category))
			{
				var categorySpec = new CategoryNameSpecification(_filter.Category);
				spec = new AndSpecification<Category>(spec, categorySpec);
            }
            if (!string.IsNullOrEmpty(_filter.SearchTerm))
            {
                var searchSpec = BuildSearchSpecification(_filter.SearchTerm.Trim());
                spec = new AndSpecification<Category>(spec, searchSpec);
            }

            return spec;
        }

		private static ISpecification<Category> BuildSearchSpecification(string searchTerm)
		{
			ISpecification<Category> spec = new CategoryNameSpecification(searchTerm);
			spec = TryAddTransactionTypeSearch(spec, searchTerm);
			return spec;
        }

		private static ISpecification<Category> TryAddTransactionTypeSearch(
			ISpecification<Category> spec,
			string searchTerm)
		{
            if (Enum.TryParse<TransactionType>(searchTerm, true, out var transactionType))
            {
                var typeSpec = new TransactionTypeFilterSpecification<Category>(transactionType);
                return new OrSpecification<Category>(spec, typeSpec);
            }

            return spec;
        }
	}
}
