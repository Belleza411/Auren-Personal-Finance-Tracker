using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Domain.Common
{
	public  interface IHasTransactionType
	{
		public TransactionType TransactionType { get; set; }
    }
}
