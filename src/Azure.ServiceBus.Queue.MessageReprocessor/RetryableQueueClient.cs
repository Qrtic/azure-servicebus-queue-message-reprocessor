using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    public class RetryableQueueClient : IReceiverClient
    {
        private readonly RetrySettings _retrySettings;
        private readonly IMessagePropertiesHelper _messagePropertiesHelper;
        private readonly IQueueClient _queueClientImplementation;
        private readonly IMessageInspector _messageInspector;
        private readonly IDateTimeProvider _dateTimeProvider;

        public RetryableQueueClient(string connectionString, string queueName, RetrySettings retrySettings)
            : this(new QueueClient(
                string.IsNullOrEmpty(connectionString)
                    ? throw new ArgumentException("Value cannot be null or empty.", nameof(connectionString))
                    : connectionString,
                string.IsNullOrEmpty(queueName)
                    ? throw new ArgumentException("Value cannot be null or empty.", nameof(queueName))
                    : queueName), retrySettings)
        {
        }

        public RetryableQueueClient(IQueueClient queueClient, RetrySettings retrySettings)
            : this(queueClient, retrySettings, new MessagePropertiesHelper(), new MessageInspector(), new DateTimeProvider())
        {
        }

        internal RetryableQueueClient(IQueueClient queueClient,
            RetrySettings retrySettings,
            IMessagePropertiesHelper messagePropertiesHelper,
            IMessageInspector messageInspector,
            IDateTimeProvider dateTimeProvider)
        {
            _queueClientImplementation = queueClient ??
                                            throw new ArgumentNullException(nameof(queueClient));
            _retrySettings = retrySettings ??
                             throw new ArgumentNullException(nameof(retrySettings));
            _messagePropertiesHelper = messagePropertiesHelper ??
                                     throw new ArgumentNullException(nameof(messagePropertiesHelper));
            _messageInspector = messageInspector ??
                                throw new ArgumentNullException(nameof(messageInspector));
            _dateTimeProvider = dateTimeProvider ??
                                throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler)
        {
            RegisterMessageHandler(handler, GetDefaultMessageHandlerOptions(exceptionReceivedHandler));
        }

        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, MessageHandlerOptions messageHandlerOptions)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            messageHandlerOptions ??= GetDefaultMessageHandlerOptions();
            if (messageHandlerOptions.AutoComplete) throw new ArgumentException(
                $"{nameof(messageHandlerOptions)}.{nameof(messageHandlerOptions.AutoComplete)} should be set to false.",
                nameof(messageHandlerOptions));
            if (messageHandlerOptions.MaxConcurrentCalls != 1) throw new ArgumentException(
                $"{nameof(messageHandlerOptions)}.{nameof(messageHandlerOptions.MaxConcurrentCalls)} should be set to 1.",
                nameof(messageHandlerOptions));

            _queueClientImplementation.RegisterMessageHandler(
                GetMessageHandlerWrapper(handler, GetExceptionHandler(messageHandlerOptions)),
                messageHandlerOptions);
        }

        public Task CloseAsync()
        {
            return _queueClientImplementation.CloseAsync();
        }

        public void RegisterPlugin(ServiceBusPlugin serviceBusPlugin)
        {
            _queueClientImplementation.RegisterPlugin(serviceBusPlugin);
        }

        public void UnregisterPlugin(string serviceBusPluginName)
        {
            _queueClientImplementation.UnregisterPlugin(serviceBusPluginName);
        }

        public string ClientId => _queueClientImplementation.ClientId;

        public bool IsClosedOrClosing => _queueClientImplementation.IsClosedOrClosing;

        public string Path => _queueClientImplementation.Path;

        public TimeSpan OperationTimeout
        {
            get => _queueClientImplementation.OperationTimeout;
            set => _queueClientImplementation.OperationTimeout = value;
        }

        public ServiceBusConnection ServiceBusConnection => _queueClientImplementation.ServiceBusConnection;

        public bool OwnsConnection => _queueClientImplementation.OwnsConnection;

        public IList<ServiceBusPlugin> RegisteredPlugins => _queueClientImplementation.RegisteredPlugins;

        public Task UnregisterMessageHandlerAsync(TimeSpan inflightMessageHandlerTasksWaitTimeout)
        {
            return _queueClientImplementation.UnregisterMessageHandlerAsync(inflightMessageHandlerTasksWaitTimeout);
        }

        public Task CompleteAsync(string lockToken)
        {
            return _queueClientImplementation.CompleteAsync(lockToken);
        }

        public Task AbandonAsync(string lockToken, IDictionary<string, object> propertiesToModify = null)
        {
            return _queueClientImplementation.AbandonAsync(lockToken, propertiesToModify);
        }

        public Task DeadLetterAsync(string lockToken, IDictionary<string, object> propertiesToModify = null)
        {
            return _queueClientImplementation.DeadLetterAsync(lockToken, propertiesToModify);
        }

        public Task DeadLetterAsync(string lockToken, string deadLetterReason, string deadLetterErrorDescription = null)
        {
            return _queueClientImplementation.DeadLetterAsync(lockToken, deadLetterReason, deadLetterErrorDescription);
        }

        public int PrefetchCount
        {
            get => _queueClientImplementation.PrefetchCount;
            set => _queueClientImplementation.PrefetchCount = value;
        }

        public ReceiveMode ReceiveMode => _queueClientImplementation.ReceiveMode;

        private Func<Message, CancellationToken, Task> GetMessageHandlerWrapper(Func<Message, CancellationToken, Task> handler,
            Func<Message, Exception, Task> exceptionHandler)
        {
            return async (message, cancellationToken) =>
            {
                try
                {
                    await handler(message, cancellationToken);

                    await _queueClientImplementation.CompleteAsync(_messageInspector.GetLockToken(message));
                }
                catch (RetryableOperationException)
                {
                    await DelayMessage(message, cancellationToken);
                }
                catch (Exception e)
                {
                    await exceptionHandler(message, e);
                }
            };
        }

        private Func<Message, Exception, Task> GetExceptionHandler(MessageHandlerOptions messageHandlerOptions)
        {
            return async (message, exception) =>
            {
                var args = new ExceptionReceivedEventArgs(
                    exception,
                    ExceptionReceivedEventArgsAction.UserCallback,
                    _queueClientImplementation.ServiceBusConnection.Endpoint.Authority,
                    _queueClientImplementation.QueueName,
                    _queueClientImplementation.ClientId);
                messageHandlerOptions.ExceptionReceivedHandler?.Invoke(args);

                await _queueClientImplementation.AbandonAsync(_messageInspector.GetLockToken(message));
            };
        }

        private MessageHandlerOptions GetDefaultMessageHandlerOptions(Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler = null)
        {
            exceptionReceivedHandler ??= args => Task.CompletedTask;
            return new MessageHandlerOptions(exceptionReceivedHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 1
            };
        }

        private async Task DelayMessage(Message message, CancellationToken _)
        {
            int attempt = _messagePropertiesHelper.GetAttempt(message) + 1;
            string lockToken = _messageInspector.GetLockToken(message);

            if (!_retrySettings.RetryDelayStrategy.CanDelay(attempt))
            {
                await _queueClientImplementation.DeadLetterAsync(lockToken, "Exceed retry attempts.");
                return;
            }

            TimeSpan delay = _retrySettings.RetryDelayStrategy.GetDelay(attempt);
            Message messageCopy = message.Clone();
            // requires to set new id if the queue checks for message duplicates
            messageCopy.MessageId = Guid.NewGuid().ToString();
            _messagePropertiesHelper.EnrichWithAttempts(messageCopy, attempt);

            using (TransactionScope transactionScope = new TransactionScope())
            {
                // If any of those operations fail it means we've lost connection to the ServiceBus
                // The next time we see this message again will be handled as it was abandoned
                await Task.WhenAll(
                    _queueClientImplementation.ScheduleMessageAsync(messageCopy, _dateTimeProvider.UtcNow + delay),
                    _queueClientImplementation.CompleteAsync(lockToken));
                transactionScope.Complete();
            }
        }
    }
}
