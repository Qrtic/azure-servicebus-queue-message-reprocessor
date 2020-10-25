using System;
using Shouldly;
using Xunit;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests
{
    public class NoneDelayStrategyTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        public void GivenNoneDelayStrategy_WhenCanDelayIsCalled_ThenReturnsFalse(int attempt)
        {
            var strategy = new NoneDelayStrategy();
            strategy.CanDelay(attempt).ShouldBeFalse();
        }

        [Fact]
        public void GivenNoneDelayStrategy_WhenGetDelayIsCalled_ThenThrows()
        {
            var strategy = new NoneDelayStrategy();
            var attempt = Utils.Random.Next();
            Should.Throw<NotImplementedException>(() => strategy.GetDelay(attempt));
        }
    }
}
