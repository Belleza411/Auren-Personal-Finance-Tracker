namespace Auren.Application.Common.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime Today { get; }
        DateTime UtcNow { get; }
    }
}
