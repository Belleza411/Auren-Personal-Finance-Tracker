using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
