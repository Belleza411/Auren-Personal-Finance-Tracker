using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auren.Application.DTOs.Responses.Category
{
	public sealed record ExpenseCategoryChartResponse(
		string Category,
		decimal Amount,
		decimal Percentage
    );
}
