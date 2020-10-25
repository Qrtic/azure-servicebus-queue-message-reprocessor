using System;
using Azure.ServiceBus.Queue.MessageReprocessor.UnitTests.Extensions;
using Shouldly;
using Xunit;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests
{
    public class LinearDelayStrategyTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-1000)]
        public void GivenInvalidMaxAttempt_WhenConstructing_ThenThrows(int maxAttempt)
        {
            Should.Throw<ArgumentOutOfRangeException>(() => new LinearDelayStrategy(maxAttempt, TimeSpan.FromMinutes(1)));
        }

        [Theory]
        [InlineData("00:00:00")]
        [InlineData("-00:00:00.001")]
        [InlineData("-1.00:00:00")]
        public void GivenInvalidDelay_WhenConstructing_ThenThrows(string delay)
        {
            Should.Throw<ArgumentOutOfRangeException>(() => new LinearDelayStrategy(1, TimeSpan.Parse(delay)));
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(4, true)]
        [InlineData(5, true)]
        [InlineData(6, false)]
        [InlineData(10, false)]
        public void GivenAttempt_WhenCanDelayIsCalled_ThenReturnsExpectedResult(int attempt, bool result)
        {
            var strategy = new LinearDelayStrategy(5, TimeSpan.FromMinutes(1));
            strategy.CanDelay(attempt).ShouldBe(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void GivenAttempt_WhenGetDelayIsCalled_ThenReturnsSameInterval(int attempt)
        {
            var delay = Utils.Random.GetTimeSpan();
            var strategy = new LinearDelayStrategy(5, delay);
            strategy.GetDelay(attempt).ShouldBe(delay);
        }

        [Theory]
        [InlineData(-100)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(100)]
        public void GivenAttemptGreaterThenMaxAttempts_WhenGetDelayIsCalled_ThenThrows(int attempt)
        {
            var strategy = new LinearDelayStrategy(5, TimeSpan.FromMinutes(1));
            Should.Throw<ArgumentOutOfRangeException>(() => strategy.GetDelay(attempt));
        }
    }
}
