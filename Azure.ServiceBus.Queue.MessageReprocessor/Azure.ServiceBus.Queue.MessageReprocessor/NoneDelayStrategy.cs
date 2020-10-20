using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    public class NoneDelayStrategy : IRetryDelayStrategy
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
