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
				   (filter.Categories != null && filter.Categories.Any());
        }

		private ISpecification<Category> ApplyFilters(ISpecification<Category> spec)
		{
            if (_filter.TransactionType.HasValue)
            {
				spec = spec.And(new TransactionTypeFilterSpecification<Category>(_filter.TransactionType.Value));
            }

            if (!string.IsNullOrEmpty(_filter.SearchTerm))
            {
				spec = spec.And(BuildSearchSpecification(_filter.SearchTerm.Trim()));
            }

            return spec;
        }

		private static ISpecification<Category> BuildSearchSpecification(string searchTerm)
		{
			ISpecification<Category> spec = new NameFilterSpecification<Category>(searchTerm);
			spec = TryAddTransactionTypeSearch(spec, searchTerm);
			return spec;
        }

		private static ISpecification<Category> TryAddTransactionTypeSearch(
			ISpecification<Category> spec,
			string searchTerm)
		{
            if (Enum.TryParse<TransactionType>(searchTerm, true, out var transactionType))
            {
				return spec.Or(new TransactionTypeFilterSpecification<Category>(transactionType));
            }

            return spec;
        }
	}
}
