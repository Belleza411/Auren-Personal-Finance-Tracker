using Auren.Domain.Enums;

namespace Auren.Application.Features.Dashboard.Queries.GetExpenseBreakdown
{
    public record GetExpenseBreakdownQuery(Guid UserId, TimePeriod TimePeriod);
}
