using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auren.Application.DTOs.Responses.Dashboard
{
	public sealed record ExpenseBreakdownResponse(
		IEnumerable<string> Labels,
		IEnumerable<decimal> Data,
		IEnumerable<decimal> Percentage,
		IEnumerable<string> BackgroundColor,
		decimal TotalSpent
    );
}
