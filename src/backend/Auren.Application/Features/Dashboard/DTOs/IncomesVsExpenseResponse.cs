namespace Auren.Application.Features.Dashboard.DTOs
{
    public sealed record IncomesVsExpenseResponse(IEnumerable<string> Labels, IEnumerable<decimal> Incomes, IEnumerable<decimal> Expenses);
}
