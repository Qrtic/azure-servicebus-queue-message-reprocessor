using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    // TODO: all sort of verification
    public class RetryableQueueClient : IReceiverClient
    {
        private const string RetryAttemptPropertyName = "retry-attempt";
        
        private readonly RetrySettings _retrySettings;
        private readonly IQueueClient _receiverClientImplementation;

        public RetryableQueueClient(string connectionString, string queueName, RetrySettings retrySettings)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Value cannot be null or empty.", nameof(connectionString));
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(queueName));
            _retrySettings = retrySettings;

            // check that queue exists
            // check if that doesn't exist we can create a new one dynamically
            _receiverClientImplementation = new QueueClient(connectionString, queueName);
        }

        public RetryableQueueClient(IQueueClient receiverClientImplementation)
        {
            _receiverClientImplementation = receiverClientImplementation ??
                                            throw new ArgumentNullException(nameof(receiverClientImplementation));
        }

        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler)
        {
            // Main code
            _receiverClientImplementation.RegisterMessageHandler(GetMessageHandlerWrapper(handler), exceptionReceivedHandler);
        }

        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, MessageHandlerOptions messageHandlerOptions)
        {
            messageHandlerOptions.AutoComplete = false;
            messageHandlerOptions.MaxConcurrentCalls = 1; // Not sure what will happen with this now
            // Main code
            _receiverClientImplementation.RegisterMessageHandler(GetMessageHandlerWrapper(handler), messageHandlerOptions);
        }

        public Task CloseAsync()
        {
            return _receiverClientImplementation.CloseAsync();
        }

        public void RegisterPlugin(ServiceBusPlugin serviceBusPlugin)
        {
            _receiverClientImplementation.RegisterPlugin(serviceBusPlugin);
        }

        public void UnregisterPlugin(string serviceBusPluginName)
        {
            _receiverClientImplementation.UnregisterPlugin(serviceBusPluginName);
        }

        public string ClientId => _receiverClientImplementation.ClientId;

        public bool IsClosedOrClosing => _receiverClientImplementation.IsClosedOrClosing;

        public string Path => _receiverClientImplementation.Path;

        public TimeSpan OperationTimeout
        {
            get => _receiverClientImplementation.OperationTimeout;
            set => _receiverClientImplementation.OperationTimeout = value;
        }

        public ServiceBusConnection ServiceBusConnection => _receiverClientImplementation.ServiceBusConnection;

        public bool OwnsConnection => _receiverClientImplementation.OwnsConnection;

        public IList<ServiceBusPlugin> RegisteredPlugins => _receiverClientImplementation.RegisteredPlugins;

        public Task UnregisterMessageHandlerAsync(TimeSpan inflightMessageHandlerTasksWaitTimeout)
        {
            return _receiverClientImplementation.UnregisterMessageHandlerAsync(inflightMessageHandlerTasksWaitTimeout);
        }

        public Task CompleteAsync(string lockToken)
        {
            return _receiverClientImplementation.CompleteAsync(lockToken);
        }

        public Task AbandonAsync(string lockToken, IDictionary<string, object> propertiesToModify = null)
        {
            return _receiverClientImplementation.AbandonAsync(lockToken, propertiesToModify);
        }

        public Task DeadLetterAsync(string lockToken, IDictionary<string, object> propertiesToModify = null)
        {
            return _receiverClientImplementation.DeadLetterAsync(lockToken, propertiesToModify);
        }

        public Task DeadLetterAsync(string lockToken, string deadLetterReason, string deadLetterErrorDescription = null)
        {
            return _receiverClientImplementation.DeadLetterAsync(lockToken, deadLetterReason, deadLetterErrorDescription);
        }

        public int PrefetchCount
        {
            get => _receiverClientImplementation.PrefetchCount;
            set => _receiverClientImplementation.PrefetchCount = value;
        }

        public ReceiveMode ReceiveMode => _receiverClientImplementation.ReceiveMode;
        
        private Func<Message, CancellationToken, Task> GetMessageHandlerWrapper(Func<Message, CancellationToken, Task> handler)
        {
            return async (message, cancellationToken) =>
            {
                try
                {
                    await handler(message, cancellationToken);
                    
                    await _receiverClientImplementation.CompleteAsync(GetLockToken(message));
                }
                catch (RetryableOperationException)
                {
                    await DelayMessage(message, cancellationToken);
                }
            };
        }

        // lock token will throw for test substitutes
        private string GetLockToken(Message message) => message.SystemProperties.LockToken;

        private int GetAttempt(Message message)
        {
            message.UserProperties.TryGetValue(RetryAttemptPropertyName, out object attempt);
            if (attempt is int attemptNumber)
            {
                return attemptNumber + 1;
            }
            return 1;
        }
        
        private async Task DelayMessage(Message message, CancellationToken cancellationToken)
        {
            int attempt = GetAttempt(message);
            string lockToken = GetLockToken(message);
            if (!_retrySettings.RetryDelayStrategy.CanDelay(attempt))
            {
                await _receiverClientImplementation.DeadLetterAsync(lockToken, "Exceed retry attempts.");
            }
            else
            {
                TimeSpan delayOn = _retrySettings.RetryDelayStrategy.GetDelay(attempt);
                Message messageCopy = message.Clone();
                // requires to set new id if the queue checks for message duplicates
                messageCopy.MessageId = Guid.NewGuid().ToString();
                messageCopy.UserProperties[RetryAttemptPropertyName] = attempt;
                messageCopy.ScheduledEnqueueTimeUtc = DateTime.UtcNow.Add(delayOn);

                using TransactionScope transactionScope = new TransactionScope();
                // If any of those operations fail it means we've lost connection to the ServiceBus
                // The next time we see this message again will be handled as it was abandoned
                await Task.WhenAll(
                    _receiverClientImplementation.SendAsync(messageCopy),
                    _receiverClientImplementation.CompleteAsync(lockToken));
                transactionScope.Complete();
            }
        }
    }
}
