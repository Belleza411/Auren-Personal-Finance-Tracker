using Auren.Application.Common.Result;
using Auren.Application.Features.Dashboard.DTOs;
using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<Result<DashboardSummaryResponse>> GetDashboardSummary(Guid userId, TimePeriod timePeriod = TimePeriod.ThisMonth, CancellationToken cancellationToken = default);
        Task<Result<IncomesVsExpenseResponse>> GetIncomesVsExpenses(
            Guid userId, TimePeriod timePeriod = TimePeriod.ThisMonth, CancellationToken cancellationToken = default);
        Task<Result<ExpenseBreakdownResponse>> GetExpenseBreakdown(
            Guid userId, TimePeriod timePeriod = TimePeriod.ThisMonth, CancellationToken cancellationToken = default);
    }
}
