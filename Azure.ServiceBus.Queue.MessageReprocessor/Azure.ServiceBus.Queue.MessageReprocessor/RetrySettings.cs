using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    public class RetrySettings
    {
        public IRetryDelayStrategy RetryDelayStrategy { get; }

        public RetrySettings(IRetryDelayStrategy retryDelayStrategy)
        {
            RetryDelayStrategy = retryDelayStrategy ?? throw new ArgumentNullException(nameof(retryDelayStrategy));
        }
    }
}
