using Auren.Domain.Enums;

namespace Auren.Application.Features.Dashboard.Queries.GetIncomesVsExpenses
{
    public record GetIncomesVsExpensesQuery(Guid UserId, TimePeriod TimePeriod);
}
