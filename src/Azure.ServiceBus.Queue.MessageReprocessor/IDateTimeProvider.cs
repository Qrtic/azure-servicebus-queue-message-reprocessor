using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}
