using Auren.Domain.Enums;

namespace Auren.Domain.Common
{
	public  interface IHasTransactionType
	{
		public TransactionType TransactionType { get; set; }
    }
}
