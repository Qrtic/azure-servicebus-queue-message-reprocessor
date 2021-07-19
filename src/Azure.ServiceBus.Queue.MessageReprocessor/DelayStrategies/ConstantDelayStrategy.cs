using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies
{
    public class ConstantDelayStrategy : DelayStrategyBase
    {
        private readonly TimeSpan _delay;

        public ConstantDelayStrategy(int maxAttempts, TimeSpan delay)
            : base(maxAttempts)
        {
            if (delay <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(delay), "Should be greater than zero.");
            }

            _delay = delay;
        }

        protected override TimeSpan GetDelayImpl(int attempt)
        {
            return _delay;
        }
    }
}
