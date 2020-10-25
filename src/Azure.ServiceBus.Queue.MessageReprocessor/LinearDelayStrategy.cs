using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    public class LinearDelayStrategy : IRetryDelayStrategy
    {
        private readonly int _maxAttempt;
        private readonly TimeSpan _delay;

        public LinearDelayStrategy(int maxAttempt, TimeSpan delay)
        {
            if (maxAttempt <= 0) throw new ArgumentOutOfRangeException(nameof(maxAttempt));
            if (delay <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));
            _maxAttempt = maxAttempt;
            _delay = delay;
        }

        public bool CanDelay(int attempt)
        {
            return attempt <= _maxAttempt;
        }

        public TimeSpan GetDelay(int attempt)
        {
            if (attempt <= 0 || attempt > _maxAttempt) throw new ArgumentOutOfRangeException(nameof(attempt));
            return _delay;
        }
    }
}
