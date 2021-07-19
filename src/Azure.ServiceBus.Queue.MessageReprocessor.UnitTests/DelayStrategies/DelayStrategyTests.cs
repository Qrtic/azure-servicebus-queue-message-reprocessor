using System;
using Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies;
using Azure.ServiceBus.Queue.MessageReprocessor.UnitTests.Extensions;
using Shouldly;
using Xunit;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests.DelayStrategies
{
    public abstract class DelayStrategyTests
    {
        protected abstract IRetryDelayStrategy GetStrategyWithMaxAttempts(int maxAttempts);

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(4, 5, true)]
        [InlineData(3, 2, false)]
        public void GivenMaxAttempts_WhenCanDelayIsCalled_ThenReturnsExpectedResult(int attempt, int maxAttempts, bool canDelay)
        {
            var target = GetStrategyWithMaxAttempts(maxAttempts);
            target.CanDelay(attempt).ShouldBe(canDelay);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void GivenAttempt_WhenGetDelayIsCalledTwice_ThenReturnsSameInterval(int attempt)
        {
            var delay = Utils.Random.GetTimeSpan();
            var strategy = new ConstantDelayStrategy(3, delay);
            var initialDelay = strategy.GetDelay(attempt);
            strategy.GetDelay(attempt).ShouldBe(initialDelay);
        }
    }

    public class ConstantDelayStrategyTests : DelayStrategyTests
    {
        protected override IRetryDelayStrategy GetStrategyWithMaxAttempts(int maxAttempts) =>
            new ConstantDelayStrategy(maxAttempts, TimeSpan.FromSeconds(1));
    }

    public class LinearDelayStrategyTests : InvalidParametersDelayStrategyTests
    {
        protected override IRetryDelayStrategy GetStrategyWithMaxAttempts(int maxAttempts) =>
            new LinearDelayStrategy(maxAttempts, TimeSpan.FromSeconds(1), 1);
    }

    public class ExponentialDelayStrategyTests : InvalidParametersDelayStrategyTests
    {
        protected override IRetryDelayStrategy GetStrategyWithMaxAttempts(int maxAttempts) =>
            new ExponentialDelayStrategy(maxAttempts, TimeSpan.FromSeconds(1), 1);
    }

    public class ExponentialWithJitterDelayStrategyTests : InvalidParametersDelayStrategyTests
    {
        protected override IRetryDelayStrategy GetStrategyWithMaxAttempts(int maxAttempts) =>
            new ExponentialWithJitterDelayStrategy(maxAttempts, TimeSpan.FromSeconds(1), 1);
    }
}
