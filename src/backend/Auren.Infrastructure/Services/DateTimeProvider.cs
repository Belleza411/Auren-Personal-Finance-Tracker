using Auren.Application.Common.Interfaces;

namespace Auren.Infrastructure.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Today => DateTime.Today;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}
