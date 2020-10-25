using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests
{
    public static class Utils
    {
        public static readonly Random Random = new Random();
        public const string ValidConnectionString =
            "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=AnyPolicy;SharedAccessKey=123";
    }
}
