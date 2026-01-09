using Auren.Application.DTOs.Filters;
using Auren.Application.Interfaces.Specification;
using Auren.Application.Specifications.Common;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Auren.Application.Specifications.Transactions
{
	public class TransactionFilterSpecification : BaseSpecification<Transaction>
	{
		private readonly Guid _userId;
		private readonly TransactionFilter _filter;

		public TransactionFilterSpecification(Guid userId, TransactionFilter filter)
		{
			_userId = userId;
			_filter = filter ?? new TransactionFilter();
		}

		public override Expression<Func<Transaction, bool>> ToExpression()
		{
            ISpecification<Transaction> spec = new UserSpecification<Transaction>(_userId);

            if (!HasActiveFilters(_filter))
                return spec.ToExpression();

            spec = ApplyFilters(spec);

            return spec.ToExpression(); 
        }

        private static bool HasActiveFilters(TransactionFilter filter)
        {
            if (filter == null) return false;

            return !string.IsNullOrWhiteSpace(filter.SearchTerm) ||
                   filter.TransactionType.HasValue ||
                   filter.MinAmount.HasValue ||
                   filter.MaxAmount.HasValue ||
                   filter.StartDate.HasValue ||
                   filter.EndDate.HasValue ||
                   filter.PaymentType.HasValue;
        }

        private ISpecification<Transaction> ApplyFilters(ISpecification<Transaction> spec)
        {
            if(_filter.TransactionType.HasValue)
            {
                spec.And(new TransactionTypeFilterSpecification<Transaction>(_filter.TransactionType.Value));
            }

            if(_filter.PaymentType.HasValue)
            {
                spec = spec.And(new PaymentTypeFilterSpecification(_filter.PaymentType.Value));
            }

            if(_filter.StartDate.HasValue && _filter.EndDate.HasValue)
            {
                spec = spec.And(new DateRangeFilterSpecification(_filter.StartDate.Value, _filter.EndDate.Value));
            }

            if(_filter.MinAmount.HasValue && _filter.MaxAmount.HasValue) 
            {
                spec = spec.And(new AmountRangeFilterspecification(_filter.MinAmount.Value, _filter.MaxAmount.Value));
            }

            if(!string.IsNullOrEmpty(_filter.SearchTerm))
            {
                spec = spec.And(BuildSearchSpecification(_filter.SearchTerm.Trim()));
            }

            return spec;
        }

        private static ISpecification<Transaction> BuildSearchSpecification(string searchTerm)
        {
            ISpecification<Transaction> spec = new SearchTermSpecification.NameSearchSpecification(searchTerm);

            spec = TryAddTransactionTypeSearch(spec, searchTerm);
            spec = TryAddPaymentTypeSearch(spec, searchTerm);
            spec = TryAddAmountSearch(spec, searchTerm);
            spec = TryAddDateSearch(spec, searchTerm);

            return spec;
        }

        private static ISpecification<Transaction> TryAddTransactionTypeSearch(
            ISpecification<Transaction> spec,
            string searchTerm)
        {
            if (Enum.TryParse<TransactionType>(searchTerm, true, out var transactionType))
            {
                return spec.Or(new TransactionTypeFilterSpecification<Transaction>(transactionType));
            }

            return spec;
        }

        private static ISpecification<Transaction> TryAddPaymentTypeSearch(
            ISpecification<Transaction> spec,
            string searchTerm)
        {
            if (Enum.TryParse<PaymentType>(searchTerm, true, out var paymentType))
            {
                return spec.Or(new PaymentTypeFilterSpecification(paymentType));
            }

            return spec;
        }

        private static ISpecification<Transaction> TryAddAmountSearch(
            ISpecification<Transaction> spec,
            string searchTerm)
        {
            if(decimal.TryParse(searchTerm, out decimal amount))
            {
                return spec.Or(new SearchTermSpecification.AmountSearchSpecification(amount));
            }

            return spec;
        }

        private static ISpecification<Transaction> TryAddDateSearch(
            ISpecification<Transaction> spec,
            string searchTerm)
        {
            if(DateTime.TryParse(searchTerm, out var date))
            {
                return spec.Or(new SearchTermSpecification.DateSearchSpecification(date));
            }

            return spec;
        }
    }
}
