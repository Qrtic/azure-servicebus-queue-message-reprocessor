using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies
{
    public class NoDelayStrategy : IRetryDelayStrategy
    {
        public bool CanDelay(int attempt)
        {
            return false;
        }

        public TimeSpan GetDelay(int attempt)
        {
            throw new NotImplementedException();
        }
    }
}
