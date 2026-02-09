using Auren.Application.Common.Result;
using Auren.Application.DTOs.Responses.Dashboard;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Services
{
    public class DashboardService(IDashboardRepository dashboardRepository) : IDashboardService
    {
        public async Task<Result<DashboardSummaryResponse>> GetDashboardSummary(Guid userId, TimePeriod timePeriod = TimePeriod.ThisMonth, CancellationToken cancellationToken = default)
            => Result.Success(await dashboardRepository.GetDashboardSummaryAsync(userId, timePeriod, cancellationToken));

        public async Task<Result<IncomesVsExpenseResponse>> GetIncomesVsExpenses(
            Guid userId, TimePeriod timePeriod = TimePeriod.ThisMonth, CancellationToken cancellationToken = default)
                => Result.Success(await dashboardRepository.GetIncomesVsExpensesAsync(userId, timePeriod, cancellationToken));
    }
}
