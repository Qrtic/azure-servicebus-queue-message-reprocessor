using Microsoft.Azure.ServiceBus;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    public interface IMessageInspector
    {
        string GetLockToken(Message message);
    }
}
