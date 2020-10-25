using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.ServiceBus;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    [ExcludeFromCodeCoverage]
    public class MessageInspector : IMessageInspector
    {
        public string GetLockToken(Message message) => message.SystemProperties.LockToken;
    }
}
