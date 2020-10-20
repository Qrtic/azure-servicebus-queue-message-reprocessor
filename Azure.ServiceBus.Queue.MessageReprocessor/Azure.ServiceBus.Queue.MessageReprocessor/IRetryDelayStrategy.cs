using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    public interface IRetryDelayStrategy
    {
        bool CanDelay(int attempt);
        TimeSpan GetDelay(int attempt);
    }
}
