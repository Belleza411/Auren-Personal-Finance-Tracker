using System.Text.Json.Serialization;

namespace Auren.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TransactionType
	{
        Income = 1,
        Expense = 2
    }
}
