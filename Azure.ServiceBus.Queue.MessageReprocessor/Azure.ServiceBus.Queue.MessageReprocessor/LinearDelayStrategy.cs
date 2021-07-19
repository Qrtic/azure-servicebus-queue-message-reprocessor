using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    public abstract class DelayStrategyBase : IRetryDelayStrategy
    {
        protected readonly int _maxAttempt;

        protected DelayStrategyBase(int maxAttempt)
        {
            if (maxAttempt <= 0) throw new ArgumentOutOfRangeException(nameof(maxAttempt));
            _maxAttempt = maxAttempt;
        }

        public bool CanDelay(int attempt) => attempt <= _maxAttempt;

        public abstract TimeSpan GetDelay(int attempt);
    }

    public class ConstantDelayStrategy : DelayStrategyBase
    {
        private readonly TimeSpan _delay;

        public ConstantDelayStrategy(int maxAttempt, TimeSpan delay)
            : base(maxAttempt)
        {
            if (delay <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));
            _delay = delay;
        }

        public override  TimeSpan GetDelay(int attempt)
        {
            return _delay;
        }
    }

    public class LinearDelayStrategy : DelayStrategyBase
    {
        private readonly TimeSpan _initialDelay;
        private readonly TimeSpan _factor;

        public LinearDelayStrategy(int maxAttempt, TimeSpan initialDelay, TimeSpan factor)
            : base(maxAttempt)
        {
            if (initialDelay <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay), "Should be greater than zero.");
            if (factor <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(factor), "Should be greater than zero.");
            _factor = factor;
        }

        public override  TimeSpan GetDelay(int attempt)
        {
            return _initialDelay + _factor * attempt;
        }
    }

    public class ExponentialDelayStrategy : DelayStrategyBase
    {
        private readonly TimeSpan _initialDelay;
        private readonly double _factor;

        public ExponentialDelayStrategy(int maxAttempt, TimeSpan initialDelay, double factor)
            : base(maxAttempt)
        {
            if (initialDelay <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay));
            if (factor <= 1) throw new ArgumentOutOfRangeException(nameof(factor));
            _initialDelay = initialDelay;
            _factor = factor;
        }

        public override  TimeSpan GetDelay(int attempt)
        {
            return _initialDelay * _factor * attempt;
        }
    }

    public class ExponentialBackoffDelayStrategy : DelayStrategyBase
    {
        private readonly TimeSpan _initialDelay;

        public ExponentialBackoffDelayStrategy(int maxAttempt, TimeSpan initialDelay, double factor, TimeSpan backoff)
            : base(maxAttempt)
        {
            if (initialDelay <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay));
            _initialDelay = initialDelay;
        }

        public override TimeSpan GetDelay(int attempt)
        {
            return _initialDelay;
        }
    }

    public class ExponentialBackoffWithJitterDelayStrategy : DelayStrategyBase
    {
        private readonly TimeSpan _delay;

        public ExponentialBackoffWithJitterDelayStrategy(int maxAttempt, TimeSpan delay)
            : base(maxAttempt)
        {
            if (delay <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));
            _delay = delay;
        }

        public override  TimeSpan GetDelay(int attempt)
        {
            return _delay;
        }
    }
}
