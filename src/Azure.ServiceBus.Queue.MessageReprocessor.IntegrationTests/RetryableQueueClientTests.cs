using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using NSubstitute;
using Xunit;

namespace Azure.ServiceBus.Queue.MessageReprocessor.IntegrationTests
{
    public class RetryableQueueClientTests : IDisposable
    {
        private readonly string _queueName;
        private readonly ServiceBusConfiguration _configuration;
        private readonly QueueClient _queueClient;

        public RetryableQueueClientTests(ServiceBusConfiguration configuration)
        {
            _queueName = "test-queue-" + Guid.NewGuid();
            _configuration = configuration;

            _queueClient = new QueueClient(
                _configuration.ListenAndSendConnectionString,
                _queueName);

            InitOrClearQueue().GetAwaiter().GetResult();
        }

        private async Task InitOrClearQueue()
        {
            var managementClient = new ManagementClient(_configuration.ManageConnectionString);
            if (!await managementClient.QueueExistsAsync(_queueName))
            {
                await managementClient.CreateQueueAsync(new QueueDescription(_queueName)
                {
                    // Requires at least Standard Tier
                    // RequiresDuplicateDetection = true,
                    // AutoDeleteOnIdle = TimeSpan.FromHours(1)
                });
            }

            await managementClient.CloseAsync();

            var messageReceiver = new MessageReceiver(
                _configuration.ListenOnlyConnectionString,
                _queueName,
                ReceiveMode.ReceiveAndDelete,
                prefetchCount: 100);

            while ((await messageReceiver.ReceiveAsync(TimeSpan.FromSeconds(5))) != null)
            {
            }

            await messageReceiver.CloseAsync();
        }

        public void Dispose()
        {
            DisposeAsync()
                .GetAwaiter()
                .GetResult();
        }

        private async Task DisposeAsync()
        {
            await _queueClient.CloseAsync();

            var managementClient = new ManagementClient(_configuration.ManageConnectionString);
            if (await managementClient.QueueExistsAsync(_queueName))
            {
                await managementClient.DeleteQueueAsync(_queueName);
            }

            await managementClient.CloseAsync();
        }

        [Fact]
        public async Task GivenOnlyFirstAttemptShouldBeRescheduled_WhenMessageReceived_ThenHandlesOnlyTwice()
        {
            var retryableOperationException = new RetryableOperationException();
            var defaultException = new Exception();
            var handler = Substitute.For<Func<Message, CancellationToken, Task>>();
            handler.Invoke(Arg.Any<Message>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException(retryableOperationException),
                    Task.FromException(defaultException));

            var retryQueue = new RetryableQueueClient(
                _configuration.ListenAndSendConnectionString,
                _queueName,
                new RetrySettings(
                    new LinearDelayStrategy(3, TimeSpan.FromSeconds(10))));

            retryQueue.RegisterMessageHandler(handler, args => Task.CompletedTask);
            await _queueClient.SendAsync(new Message());

            await Task.Delay(TimeSpan.FromSeconds(30));

            await handler.Received(2).Invoke(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GivenSingleMaxAttemptForRescheduling_WhenMessageReceived_ThenHandlesOnlyTwice()
        {
            var retryableOperationException = new RetryableOperationException();
            var handler = Substitute.For<Func<Message, CancellationToken, Task>>();
            handler.Invoke(Arg.Any<Message>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException(retryableOperationException),
                    Task.FromException(retryableOperationException));

            var retryQueue = new RetryableQueueClient(
                _configuration.ListenAndSendConnectionString,
                _queueName,
                new RetrySettings(
                    new LinearDelayStrategy(1, TimeSpan.FromSeconds(10))));

            retryQueue.RegisterMessageHandler(handler, args => Task.CompletedTask);
            await _queueClient.SendAsync(new Message());

            await Task.Delay(TimeSpan.FromSeconds(30));

            await handler.Received(2).Invoke(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        }
    }
}
