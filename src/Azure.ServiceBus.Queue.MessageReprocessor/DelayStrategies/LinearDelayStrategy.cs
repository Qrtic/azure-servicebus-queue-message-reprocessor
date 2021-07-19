using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies
{
    public class LinearDelayStrategy : DelayStrategyBase
    {
        private readonly TimeSpan _initialDelay;
        private readonly double _factor;

        public LinearDelayStrategy(int maxAttempts, TimeSpan initialDelay, double factor)
            : base(maxAttempts)
        {
            if (initialDelay <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(initialDelay), "Should be greater than zero.");
            }

            if (factor < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(factor), "Should be greater or equal to 1.");
            }

            _initialDelay = initialDelay;
            _factor = factor;
        }

        protected override TimeSpan GetDelayImpl(int attempt)
        {
            return _initialDelay * _factor * attempt;
        }
    }
}
