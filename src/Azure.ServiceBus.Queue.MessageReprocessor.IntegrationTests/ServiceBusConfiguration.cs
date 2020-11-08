namespace Azure.ServiceBus.Queue.MessageReprocessor.IntegrationTests
{
    public class ServiceBusConfiguration
    {
        public string ListenOnlyConnectionString { get; set; }
        public string SendOnlyConnectionString { get; set; }
        public string ListenAndSendConnectionString { get; set; }
        public string ManageConnectionString { get; set; }
    }
}
