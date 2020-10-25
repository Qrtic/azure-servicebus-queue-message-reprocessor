using System;
using System.Diagnostics.CodeAnalysis;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    [ExcludeFromCodeCoverage]
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
