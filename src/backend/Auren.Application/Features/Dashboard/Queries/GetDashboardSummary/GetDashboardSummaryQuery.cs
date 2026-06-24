using Auren.Domain.Enums;

namespace Auren.Application.Features.Dashboard.Queries.GetDashboardSummary
{
    public record GetDashboardSummaryQuery(Guid UserId, TimePeriod TimePeriod);
}
