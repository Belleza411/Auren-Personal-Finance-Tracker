using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auren.Application.Features.Dashboard.DTOs
{
	public sealed record BalanceSummaryResponse(decimal Income, decimal Expense, decimal Balance);
}
