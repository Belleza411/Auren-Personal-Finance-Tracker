using System.Text.Json.Serialization;

namespace Auren.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TimePeriod
	{
		AllTime,
		ThisMonth,
		LastMonth,
		Last3Months,
		Last6Months,
		ThisYear
	}
}
