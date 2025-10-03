namespace Auren.API.Models.Domain
{
	public class MonthlyDataPoint
	{
		public DateTime Month { get; set; }
		public decimal Income { get; set; }
		public decimal Expense { get; set; }
    }
}
