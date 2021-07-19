using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies
{
    public class ExponentialDelayStrategy : DelayStrategyBase
    {
        private readonly TimeSpan _initialDelay;
        private readonly double _exponent;

        public ExponentialDelayStrategy(int maxAttempts, TimeSpan initialDelay, double exponent)
            : base(maxAttempts)
        {
            if (initialDelay <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(initialDelay), "Should be greater than zero.");
            }

            if (exponent < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(exponent), "Should be greater or equal to 1.");
            }

            _initialDelay = initialDelay;
            _exponent = exponent;
        }

        protected override TimeSpan GetDelayImpl(int attempt)
        {
            return _initialDelay * Math.Pow(_exponent, attempt);
        }
    }
}
