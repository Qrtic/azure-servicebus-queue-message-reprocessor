using Microsoft.Azure.ServiceBus;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    public interface IMessagePropertiesHelper
    {
        void EnrichWithAttempts(Message message, int attempt);
        int GetAttempt(Message message);
    }
}
