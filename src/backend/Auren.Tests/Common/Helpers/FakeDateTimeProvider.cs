using Auren.Application.Common.Interfaces;

namespace Auren.Tests.Common.Helpers
{
    public class FakeDateTimeProvider(DateTime fixedDate) : IDateTimeProvider
    {
        public DateTime Today => fixedDate.Date;

        public DateTime UtcNow => fixedDate;
    }
}
