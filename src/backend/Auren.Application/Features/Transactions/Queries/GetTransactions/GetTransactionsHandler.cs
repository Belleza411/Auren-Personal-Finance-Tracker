using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Models;
using Auren.Application.Common.Result;
using Auren.Application.Common.Specifications.Transactions;
using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Auren.Application.Features.Transactions.Queries.GetTransactions
{
    public class GetTransactionsHandler(IAppDbContext db)
    {
        public async Task<Result<PagedResult<Transaction>>> Handle(
            GetTransactionsQuery query,
            CancellationToken ct)
        {
            IEnumerable<Guid> categoryIds = [];
            if (query.Filter.Category?.Any() == true)
            {
                categoryIds = await db.Categories
                    .Where(c =>
                        c.UserId == query.UserId &&
                        query.Filter.Category.Contains(c.Name))
                    .Select(c => c.Id)
                    .ToListAsync(ct);
            }

            var spec = new TransactionFilterSpecification(query.UserId, query.Filter, categoryIds);

            var baseQuery = db.Transactions
                .Where(spec.ToExpression())
                .Include(t => t.Category);

            var totalCount = await baseQuery.CountAsync(ct);

            var items = await baseQuery
                .OrderByDescending(t => t.TransactionDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            return Result.Success(new PagedResult<Transaction>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            });
        }
    }
}
