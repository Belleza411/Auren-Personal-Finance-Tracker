using Auren.API.Models.Enums;

namespace Auren.API.DTOs.Responses
{
	public class CategoryOverviewResponse
	{
		public string Category { get; set; } = string.Empty;
        public TransactionType TransactionType { get; set; }
		public double AverageSpending { get; set; } 
		public int TransactionCount { get; set; }
    }
}
