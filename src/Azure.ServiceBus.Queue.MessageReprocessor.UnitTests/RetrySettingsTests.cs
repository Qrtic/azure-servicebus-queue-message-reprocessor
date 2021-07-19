using System;
using Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests
{
    public class RetrySettingsTests
    {
        [Fact]
        public void GivenNullRetryDelayStrategy_WhenConstructing_ThenThrows()
        {
            Should.Throw<ArgumentException>(() => new RetrySettings(null));
        }

        [Fact]
        public void Given()
        {
            var retryDelayStrategy = Substitute.For<IRetryDelayStrategy>();
            var retrySettings = new RetrySettings(retryDelayStrategy);

            retrySettings.RetryDelayStrategy.ShouldBe(retryDelayStrategy);
        }
    }
}
