using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.DTOs.Responses.Dashboard
{
    public sealed record IncomesVsExpenseResponse(IEnumerable<string> Labels, IEnumerable<decimal> Incomes, IEnumerable<decimal> Expenses);
}
