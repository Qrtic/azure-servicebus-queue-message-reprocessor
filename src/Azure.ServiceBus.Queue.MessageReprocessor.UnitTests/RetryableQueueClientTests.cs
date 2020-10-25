using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.ServiceBus.Queue.MessageReprocessor.UnitTests.Extensions;
using Microsoft.Azure.ServiceBus;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests
{
    public class RetryableQueueClientTests
    {
        private readonly IQueueClient _queueClientImplementation;
        private readonly IMessagePropertiesHelper _messagePropertiesHelper;
        private readonly IMessageInspector _messageInspector;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly RetrySettings _retrySettings;
        private readonly IRetryDelayStrategy _retryDelayStrategy;
        private readonly Message _message;
        private readonly string _lockToken;
        private readonly int _attempt;
        private Message _scheduledMessage;
        private Func<Message, CancellationToken, Task> _receiveMessageFunc;

        public RetryableQueueClientTests()
        {
            _lockToken = Utils.Random.GetString();
            _message = new Message();
            _messagePropertiesHelper = Substitute.For<IMessagePropertiesHelper>();
            _attempt = Utils.Random.Next(2, int.MaxValue);
            _messagePropertiesHelper.GetAttempt(_message).Returns(_attempt - 1);
            _messageInspector = Substitute.For<IMessageInspector>();
            _messageInspector.GetLockToken(_message).Returns(_lockToken);
            _retryDelayStrategy = Substitute.For<IRetryDelayStrategy>();
            _retrySettings = new RetrySettings(_retryDelayStrategy);
            _dateTimeProvider = Substitute.For<IDateTimeProvider>();
            _queueClientImplementation = Substitute.For<IQueueClient>();
            _queueClientImplementation
                .WhenForAnyArgs(queueClient =>
                    queueClient.RegisterMessageHandler(null, (MessageHandlerOptions)null))
                .Do(callInfo => _receiveMessageFunc = callInfo.Arg<Func<Message, CancellationToken, Task>>());
            _queueClientImplementation
                .WhenForAnyArgs(client => client.ScheduleMessageAsync(null, default))
                .Do(callInfo => _scheduledMessage = callInfo.Arg<Message>());
            var serviceBusConnection = new ServiceBusConnection(Utils.ValidConnectionString);
            _queueClientImplementation.ServiceBusConnection
                .Returns(serviceBusConnection);
        }

        [Fact]
        public void GivenExceptionHandler_WhenRegisterMessageHandlerIsCalled_ThenRegistersMessageHandlerWithDefaultOptions()
        {
            var target = ConstructTarget();

            Func<Message, CancellationToken, Task> handler = Substitute.For<Func<Message, CancellationToken, Task>>();
            handler.Invoke(Arg.Any<Message>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            Func<ExceptionReceivedEventArgs, Task> exceptionHandler = (args) => Task.CompletedTask;
            var cancellationToken = new CancellationToken();
            target.RegisterMessageHandler(handler, exceptionHandler);
            _receiveMessageFunc(_message, cancellationToken);

            _queueClientImplementation.Received().RegisterMessageHandler(Arg.Any<Func<Message, CancellationToken, Task>>(),
                Arg.Is<MessageHandlerOptions>(
                    options => options.AutoComplete == false && options.MaxConcurrentCalls == 1));

            handler.Received().Invoke(_message, cancellationToken);
        }

        [Fact]
        public void GivenNullMessageHandlerOptions_WhenRegisterMessageHandlerIsCalled_ThenRegistersMessageHandlerWithDefaultOptions()
        {
            var target = ConstructTarget();

            Func<Message, CancellationToken, Task> handler = Substitute.For<Func<Message, CancellationToken, Task>>();
            handler.Invoke(Arg.Any<Message>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            var cancellationToken = new CancellationToken();
            target.RegisterMessageHandler(handler, (MessageHandlerOptions)null);
            _receiveMessageFunc(_message, cancellationToken);

            _queueClientImplementation.Received().RegisterMessageHandler(Arg.Any<Func<Message, CancellationToken, Task>>(),
                Arg.Is<MessageHandlerOptions>(
                    options => options.AutoComplete == false
                               && options.MaxConcurrentCalls == 1
                               && options.ExceptionReceivedHandler != null));

            handler.Received().Invoke(_message, cancellationToken);
        }

        [Fact]
        public void GivenNullMessageHandler_WhenRegisterMessageHandlerIsCalled_ThenThrows()
        {
            var target = ConstructTarget();

            Should.Throw<ArgumentNullException>(() => target.RegisterMessageHandler(null, (MessageHandlerOptions)null))
                .ParamName.ShouldBe("handler");
        }

        [Fact]
        public void GivenMessageHandlerOptionsWithInvalidAutoComplete_WhenRegisterMessageHandlerIsCalled_ThenThrows()
        {
            var target = ConstructTarget();
            Func<Message, CancellationToken, Task> handler = Substitute.For<Func<Message, CancellationToken, Task>>();
            Func<ExceptionReceivedEventArgs, Task> exceptionHandler = (args) => Task.CompletedTask;
            var messageHandlerOptions = new MessageHandlerOptions(exceptionHandler)
            {
                AutoComplete = true,
                MaxConcurrentCalls = 1
            };

            Should.Throw<ArgumentException>(() => target.RegisterMessageHandler(handler, messageHandlerOptions))
                .Message.ShouldBe("messageHandlerOptions.AutoComplete should be set to false. (Parameter 'messageHandlerOptions')");
        }

        [Fact]
        public void GivenNullMessageHandlerOptionsWithInvalidAutoComplete_WhenRegisterMessageHandlerIsCalled_ThenThrows()
        {
            var target = ConstructTarget();
            Func<Message, CancellationToken, Task> handler = Substitute.For<Func<Message, CancellationToken, Task>>();
            Func<ExceptionReceivedEventArgs, Task> exceptionHandler = (args) => Task.CompletedTask;
            var messageHandlerOptions = new MessageHandlerOptions(exceptionHandler)
            {
                AutoComplete = true,
                MaxConcurrentCalls = 1
            };

            Should.Throw<ArgumentException>(() => target.RegisterMessageHandler(handler, messageHandlerOptions))
                .Message.ShouldBe("messageHandlerOptions.AutoComplete should be set to false. (Parameter 'messageHandlerOptions')");
        }

        [Theory]
        [InlineData(2)]
        [InlineData(10)]
        public void GivenMessageHandlerOptionsWithInvalidMaxConcurrentCalls_WhenRegisterMessageHandlerIsCalled_ThenThrows(int maxConcurrentCalls)
        {
            var target = ConstructTarget();
            Func<Message, CancellationToken, Task> handler = Substitute.For<Func<Message, CancellationToken, Task>>();
            Func<ExceptionReceivedEventArgs, Task> exceptionHandler = (args) => Task.CompletedTask;
            var messageHandlerOptions = new MessageHandlerOptions(exceptionHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = maxConcurrentCalls
            };

            Should.Throw<ArgumentException>(() => target.RegisterMessageHandler(handler, messageHandlerOptions))
                .Message.ShouldBe("messageHandlerOptions.MaxConcurrentCalls should be set to 1. (Parameter 'messageHandlerOptions')");
        }

        [Fact]
        public void GivenRegisteredMessageHandler_WhenUnregisterMessageHandlerIsCalled_ThenUnerigestersImplementation()
        {
            var target = ConstructTarget();
            Func<Message, CancellationToken, Task> handler = Substitute.For<Func<Message, CancellationToken, Task>>();
            Func<ExceptionReceivedEventArgs, Task> exceptionHandler = (args) => Task.CompletedTask;
            var inflightMessageHandlerTasksWaitTimeout = Utils.Random.GetTimeSpan();
            target.RegisterMessageHandler(handler, exceptionHandler);

            target.UnregisterMessageHandlerAsync(inflightMessageHandlerTasksWaitTimeout);

            _queueClientImplementation.Received().UnregisterMessageHandlerAsync(inflightMessageHandlerTasksWaitTimeout);
        }

        [Fact]
        public async Task GivenSubscribedToQueue_WhenMessageProcessingSucceed_ThenCompletesMessage()
        {
            var target = ConstructTarget();

            target.RegisterMessageHandler(
                (message, cancellationToken) => Task.CompletedTask,
                args => Task.CompletedTask);
            await _receiveMessageFunc(_message, CancellationToken.None);

            await _queueClientImplementation.Received()
                .CompleteAsync(_lockToken);
        }

        [Fact]
        public async Task GivenMessage_WhenMessageProcessingFailedWithAnyException_ThenAbandonsMessage()
        {
            var target = ConstructTarget();

            target.RegisterMessageHandler(
                (message, cancellationToken) => Task.FromException(new Exception()),
                args => Task.CompletedTask);
            await _receiveMessageFunc(_message, CancellationToken.None);

            await _queueClientImplementation.Received()
                .AbandonAsync(_lockToken);
        }

        [Fact]
        public async Task GivenMessageCantBeDelayed_WhenMessageProcessingFailedWithRetryableOperationException_ThenDeadLettersMessage()
        {
            _retryDelayStrategy.CanDelay(_attempt).Returns(false);
            var target = ConstructTarget();

            target.RegisterMessageHandler(
                (message, cancellationToken) => Task.FromException(new RetryableOperationException()),
                args => Task.CompletedTask);
            await _receiveMessageFunc(_message, CancellationToken.None);

            await _queueClientImplementation.Received()
                .DeadLetterAsync(_lockToken, "Exceed retry attempts.");
        }

        [Fact]
        public async Task GivenDelayStrategy_WhenMessageProcessingFailedWithRetryableOperationException_ThenCompletesInitialMessage()
        {
            _retryDelayStrategy.CanDelay(_attempt).Returns(true);
            var delay = Utils.Random.GetTimeSpan();
            var now = DateTimeOffset.UtcNow;
            _dateTimeProvider.UtcNow.Returns(now);
            _retryDelayStrategy.GetDelay(_attempt).Returns(delay);
            var target = ConstructTarget();

            target.RegisterMessageHandler(
                (message, cancellationToken) => Task.FromException(new RetryableOperationException()),
                args => Task.CompletedTask);
            await _receiveMessageFunc(_message, CancellationToken.None);

            await _queueClientImplementation.Received()
                .CompleteAsync(_lockToken);
        }

        [Fact]
        public async Task GivenDelayStrategy_WhenMessageProcessingFailedWithRetryableOperationException_ThenReschedulesMessageCopyWithIncrementedAttempts()
        {
            _retryDelayStrategy.CanDelay(_attempt).Returns(true);
            var delay = Utils.Random.GetTimeSpan();
            var now = DateTimeOffset.UtcNow;
            _dateTimeProvider.UtcNow.Returns(now);
            _retryDelayStrategy.GetDelay(_attempt).Returns(delay);
            var target = ConstructTarget();

            target.RegisterMessageHandler(
                (message, cancellationToken) => Task.FromException(new RetryableOperationException()),
                args => Task.CompletedTask);
            await _receiveMessageFunc(_message, CancellationToken.None);

            await _queueClientImplementation.Received()
                .ScheduleMessageAsync(Arg.Any<Message>(), now + delay);

            _scheduledMessage.ShouldNotBeNull();
            _scheduledMessage.ShouldNotBeSameAs(_message);
            _scheduledMessage.MessageId.ShouldNotBe(_message.MessageId);
            _scheduledMessage.Body.ShouldBe(_message.Body);

            _messagePropertiesHelper.Received().EnrichWithAttempts(_scheduledMessage, _attempt);
        }

        private RetryableQueueClient ConstructTarget() => new RetryableQueueClient(
            _queueClientImplementation,
            _retrySettings,
            _messagePropertiesHelper,
            _messageInspector,
            _dateTimeProvider);
    }
}
