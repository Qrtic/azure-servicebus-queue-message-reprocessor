using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies
{
    public class ExponentialWithJitterDelayStrategy : DelayStrategyBase
    {
        private readonly TimeSpan _initialDelay;
        private readonly double _exponent;
        private readonly Random _random = new Random();
        private readonly object _randomSyncObj = new object();

        public ExponentialWithJitterDelayStrategy(int maxAttempts, TimeSpan initialDelay, double exponent, int? seed = null)
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
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        protected override TimeSpan GetDelayImpl(int attempt)
        {
            return TimeSpan.FromMilliseconds(
                UniformRandom(_initialDelay.TotalMilliseconds,
                Math.Pow(_exponent, attempt)));
        }

        private double UniformRandom(double a, double b)
        {
            if (a == b) return a;

            lock (_randomSyncObj)
            {
                return a + (b - a) * _random.NextDouble();
            }
        }
    }
}
