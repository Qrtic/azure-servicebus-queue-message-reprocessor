using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies
{
    public abstract class DelayStrategyBase : IRetryDelayStrategy
    {
        protected readonly int _maxAttempts;

        protected DelayStrategyBase(int maxAttempts)
        {
            if (maxAttempts < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Should be greater or equal to 1.");
            }
            _maxAttempts = maxAttempts;
        }

        public bool CanDelay(int attempt)
        {
            if (attempt <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attempt), "Should be greater or equal to 1.");
            }
            return attempt <= _maxAttempts;
        }

        public TimeSpan GetDelay(int attempt)
        {
            if (attempt <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attempt), "Should be greater or equal to 1.");
            }

            if (attempt > _maxAttempts)
            {
                throw new ArgumentOutOfRangeException(nameof(attempt), $"Should be less then maximum attempts '{_maxAttempts}' count.");
            }

            return GetDelayImpl(attempt);
        }

        protected abstract TimeSpan GetDelayImpl(int attempt);
    }
}
