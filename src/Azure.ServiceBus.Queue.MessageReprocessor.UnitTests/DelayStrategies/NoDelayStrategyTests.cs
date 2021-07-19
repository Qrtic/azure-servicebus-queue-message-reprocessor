using System;
using Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies;
using Shouldly;
using Xunit;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests.DelayStrategies
{
    public class NoDelayStrategyTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        public void GivenNoneDelayStrategy_WhenCanDelayIsCalled_ThenReturnsFalse(int attempt)
        {
            var strategy = new NoDelayStrategy();
            strategy.CanDelay(attempt).ShouldBeFalse();
        }

        [Fact]
        public void GivenNoneDelayStrategy_WhenGetDelayIsCalled_ThenThrows()
        {
            var strategy = new NoDelayStrategy();
            var attempt = Utils.Random.Next();
            Should.Throw<NotImplementedException>(() => strategy.GetDelay(attempt));
        }
    }
}
