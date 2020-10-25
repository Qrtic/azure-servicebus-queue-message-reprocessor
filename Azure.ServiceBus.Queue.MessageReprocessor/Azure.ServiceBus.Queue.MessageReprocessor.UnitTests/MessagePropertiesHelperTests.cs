using Azure.ServiceBus.Queue.MessageReprocessor.UnitTests.Extensions;
using Microsoft.Azure.ServiceBus;
using Shouldly;
using Xunit;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests
{
    public class MessagePropertiesHelperTests
    {
        private const string RetryAttemptPropertyName = "retry-attempt";

        private readonly MessagePropertiesHelper _target = new MessagePropertiesHelper();

        [Fact]
        public void GivenAttemptPropertyAbsent_WhenGetAttemptIsCalled_ThenReturnsZeroAttempt()
        {
            var message = CreateMessage();
            _target.GetAttempt(message).ShouldBe(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GivenAttempts_WhenGetAttemptIsCalled_ThenReturnsAttempts(int attempts)
        {
            var message = CreateMessage(attempts);
            _target.GetAttempt(message).ShouldBe(attempts);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(1)]
        [InlineData(10)]
        public void GivenMessage_WhenEnrichWithAttemptsIsCalled_ThenUpdatesAttemptsProperty(int? initialAttempts)
        {
            var attempt = 50;
            var message = CreateMessage(initialAttempts);
            _target.EnrichWithAttempts(message, attempt);

            message.UserProperties.ShouldContainKeyAndValue(RetryAttemptPropertyName, attempt);
        }

        private Message CreateMessage(int? attempts = null)
        {
            var message = new Message();
            if (attempts.HasValue)
            {
                message.UserProperties[RetryAttemptPropertyName] = attempts;
            }

            return message;
        }
    }
}
