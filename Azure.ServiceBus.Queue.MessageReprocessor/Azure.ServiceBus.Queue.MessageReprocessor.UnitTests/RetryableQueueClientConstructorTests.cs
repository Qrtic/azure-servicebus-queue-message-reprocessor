using System;
using Microsoft.Azure.ServiceBus;
using Shouldly;
using Xunit;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests
{
    public class RetryableQueueClientConstructorTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GivenNullOrEmptyConnectionString_WhenConstructing_ThenThrows(string connectionString)
        {
            Should.Throw<ArgumentException>(() =>
                    new RetryableQueueClient(connectionString, "queue", new RetrySettings(new NoneDelayStrategy())))
                .Message.ShouldBe("Value cannot be null or empty. (Parameter 'connectionString')");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GivenNullOrEmptyQueueName_WhenConstructing_ThenThrows(string queueName)
        {
            Should.Throw<ArgumentException>(() =>
                    new RetryableQueueClient(Utils.ValidConnectionString, queueName, new RetrySettings(new NoneDelayStrategy())))
                .Message.ShouldBe("Value cannot be null or empty. (Parameter 'queueName')");
        }

        [Fact]
        public void GivenNullRetrySettings_WhenConstructing_ThenThrows()
        {
            Should.Throw<ArgumentNullException>(() =>
                    new RetryableQueueClient(Utils.ValidConnectionString, "queueName", null))
                .ParamName.ShouldBe("retrySettings");
        }

        [Fact]
        public void GivenNullQueueClient_WhenConstructing_ThenThrows()
        {
            Should.Throw<ArgumentNullException>(() =>
                    new RetryableQueueClient(null, new RetrySettings(new NoneDelayStrategy())))
                .ParamName.ShouldBe("queueClient");
        }

        [Fact]
        public void GivenNullRetrySettingsForQueueClientImplementation_WhenConstructing_ThenThrows()
        {
            Should.Throw<ArgumentNullException>(() =>
                    new RetryableQueueClient(null, null))
                .ParamName.ShouldBe("queueClient");
        }
    }
}
