using Auren.Application.DTOs.Responses.Dashboard;
using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid userId, TimePeriod? timePeriod, CancellationToken cancellationToken);
        Task<IncomesVsExpenseResponse> GetIncomesVsExpensesAsync(Guid userId, TimePeriod? timePeriod, CancellationToken cancellationToken);
    }
}
