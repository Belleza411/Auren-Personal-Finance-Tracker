using Auren.Domain.Enums;

namespace Auren.Application.Constants
{
	public static class CategorySeeder
	{
		public static readonly List<(string Name, TransactionType transactionType)> DefaultCategories = new()
		{
			("Food", TransactionType.Expense),
			("Rent", TransactionType.Expense),
			("Utilities", TransactionType.Expense),
			("Transportation", TransactionType.Expense),
			("Entertainment", TransactionType.Expense),
			("Healthcare", TransactionType.Expense),
			("Education", TransactionType.Expense),
			("Groceries", TransactionType.Expense),
			("Travel", TransactionType.Expense),
			("Salary", TransactionType.Income),
			("Investments", TransactionType.Income),
			("Gifts", TransactionType.Income),
			("Miscalleanuous", TransactionType.Expense),
			("Bussiness", TransactionType.Expense),
			("Shopping", TransactionType.Expense),
            ("Other Income", TransactionType.Income)
        };
	}
}
