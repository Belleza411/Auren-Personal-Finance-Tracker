using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Dashboard.DTOs
{
    public sealed record IncomesVsExpenseResponse(IEnumerable<string> Labels, IEnumerable<decimal> Incomes, IEnumerable<decimal> Expenses);
}
