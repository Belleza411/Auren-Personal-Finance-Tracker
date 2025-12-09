using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auren.Application.DTOs.Responses.Transaction
{
	public sealed record BalanceSummaryResponse(decimal Income, decimal Expense, decimal Balance);
}
