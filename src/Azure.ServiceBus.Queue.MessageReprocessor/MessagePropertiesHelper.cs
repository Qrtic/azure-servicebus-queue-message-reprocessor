using Microsoft.Azure.ServiceBus;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    public class MessagePropertiesHelper : IMessagePropertiesHelper
    {
        private const string RetryAttemptPropertyName = "retry-attempt";

        public void EnrichWithAttempts(Message message, int attempt)
        {
            message.UserProperties[RetryAttemptPropertyName] = attempt;
        }

        public int GetAttempt(Message message)
        {
            message.UserProperties.TryGetValue(RetryAttemptPropertyName, out object attempt);
            if (attempt is int attemptNumber)
            {
                return attemptNumber;
            }
            return 0;
        }
    }
}
