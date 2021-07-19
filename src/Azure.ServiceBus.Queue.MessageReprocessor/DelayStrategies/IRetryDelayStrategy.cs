using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies
{
    public interface IRetryDelayStrategy
    {
        bool CanDelay(int attempt);
        TimeSpan GetDelay(int attempt);
    }
}
