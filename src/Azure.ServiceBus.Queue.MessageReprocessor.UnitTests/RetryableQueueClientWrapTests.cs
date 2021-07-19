using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies;
using Azure.ServiceBus.Queue.MessageReprocessor.UnitTests.Extensions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Shouldly;
using Xunit;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests
{
    public class RetryableQueueClientWrapTests
    {
        private readonly RetryableQueueClient _target;
        private readonly IQueueClient _implementation;

        public RetryableQueueClientWrapTests()
        {
            _implementation = Substitute.For<IQueueClient>();
            _target = new RetryableQueueClient(_implementation,
                new RetrySettings(Substitute.For<IRetryDelayStrategy>()));
        }

        [Fact]
        public void GivenImplementation_WhenClientIdIsRetrieved_ThenReturnsClientIdFromImplementation()
        {
            var clientId = Utils.Random.GetString();
            _implementation.ClientId.Returns(clientId);
            _target.ClientId.ShouldBe(clientId);
            var _ = _implementation.Received().ClientId;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenImplementation_WhenIsClosedOrClosingIsRetrieved_ThenReturnsIsClosedOrClosingFromImplementation(
            bool ownsConnection)
        {
            _implementation.IsClosedOrClosing.Returns(ownsConnection);
            _target.IsClosedOrClosing.ShouldBe(ownsConnection);
            var _ = _implementation.Received().IsClosedOrClosing;
        }

        [Fact]
        public void GivenImplementation_WhenPathIsRetrieved_ThenReturnsPathFromImplementation()
        {
            var path = Utils.Random.GetString();
            _implementation.Path.Returns(path);
            _target.Path.ShouldBe(path);
            var _ = _implementation.Received().Path;
        }

        [Fact]
        public void GivenImplementation_WhenOperationTimeoutIsRetrieved_ThenReturnsOperationTimeoutFromImplementation()
        {
            var operationTimeout = Utils.Random.GetTimeSpan();
            _implementation.OperationTimeout.Returns(operationTimeout);
            _target.OperationTimeout.ShouldBe(operationTimeout);
            var _ = _implementation.Received().OperationTimeout;
        }

        [Fact]
        public void GivenImplementation_WhenOperationTimeoutIsSet_ThenSetsOperationTimeoutToImplementation()
        {
            var operationTimeout = Utils.Random.GetTimeSpan();
            _target.OperationTimeout = operationTimeout;
            _implementation.Received().OperationTimeout = operationTimeout;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenImplementation_WhenOwnsConnectionIsRetrieved_ThenReturnsOwnsConnectionFromImplementation(
            bool ownsConnection)
        {
            _implementation.OwnsConnection.Returns(ownsConnection);
            _target.OwnsConnection.ShouldBe(ownsConnection);
            var _ = _implementation.Received().OwnsConnection;
        }

        [Fact]
        public void GivenImplementation_WhenPrefetchCountIsRetrieved_ThenReturnsPrefetchCountFromImplementation()
        {
            var prefetchCount = Utils.Random.GetInt();
            _implementation.PrefetchCount.Returns(prefetchCount);
            _target.PrefetchCount.ShouldBe(prefetchCount);
            var _ = _implementation.Received().PrefetchCount;
        }

        [Fact]
        public void GivenImplementation_WhenPrefetchCountIsSet_ThenSetsPrefetchCountToImplementation()
        {
            var prefetchCount = Utils.Random.GetInt();
            _target.PrefetchCount = prefetchCount;
            _implementation.Received().PrefetchCount = prefetchCount;
        }

        [Fact]
        public void GivenImplementation_WhenReceiveModeIsRetrieved_ThenReturnsReceiveModeFromImplementation()
        {
            var receiveMode = Utils.Random.GetEnum<ReceiveMode>();
            _implementation.ReceiveMode.Returns(receiveMode);
            _target.ReceiveMode.ShouldBe(receiveMode);
            var _ = _implementation.Received().ReceiveMode;
        }

        [Fact]
        public void GivenImplementation_WhenRegisteredPluginsIsRetrieved_ThenReturnsRegisteredPluginsFromImplementation()
        {
            var registeredPlugins = new List<ServiceBusPlugin>();
            _implementation.RegisteredPlugins.Returns(registeredPlugins);
            _target.RegisteredPlugins.ShouldBe(registeredPlugins);
            var _ = _implementation.Received().RegisteredPlugins;
        }

        [Fact]
        public void GivenImplementation_WhenServiceBusConnectionIsRetrieved_ThenReturnsServiceBusConnectionFromImplementation()
        {
            var serviceBusConnection = new ServiceBusConnection(Utils.ValidConnectionString);
            _implementation.ServiceBusConnection.Returns(serviceBusConnection);
            _target.ServiceBusConnection.ShouldBe(serviceBusConnection);
            var _ = _implementation.Received().ServiceBusConnection;
        }

        [Fact]
        public async Task GivenImplementation_WhenCompleteAsyncIsCalled_ThenCompletesImplementation()
        {
            var lockToken = Utils.Random.GetString();
            await _target.CompleteAsync(lockToken);
            await _implementation.Received().CompleteAsync(lockToken);
        }

        [Fact]
        public async Task GivenImplementation_WhenAbandonAsyncIsCalled_ThenAbandonssImplementation()
        {
            var lockToken = Utils.Random.GetString();
            await _target.AbandonAsync(lockToken);
            await _implementation.Received().AbandonAsync(lockToken);
        }

        [Fact]
        public async Task GivenImplementation_WhenAbandonAsyncWithPropertiesToModifyIsCalled_ThenAbandonssImplementation()
        {
            var lockToken = Utils.Random.GetString();
            var propertiesToModify = new Dictionary<string, object>();
            await _target.AbandonAsync(lockToken, propertiesToModify);
            await _implementation.Received().AbandonAsync(lockToken, propertiesToModify);
        }

        [Fact]
        public async Task GivenImplementation_WhenDeadLetterAsyncIsCalled_ThenDeadLettersImplementation()
        {
            var lockToken = Utils.Random.GetString();
            await _target.DeadLetterAsync(lockToken);
            await _implementation.Received().DeadLetterAsync(lockToken);
        }

        [Fact]
        public async Task GivenImplementation_WhenDeadLetterAsyncWithPropertiesToModifyIsCalled_ThenDeadLettersImplementation()
        {
            var lockToken = Utils.Random.GetString();
            var propertiesToModify = new Dictionary<string, object>();
            await _target.DeadLetterAsync(lockToken, propertiesToModify);
            await _implementation.Received().DeadLetterAsync(lockToken, propertiesToModify);
        }

        [Fact]
        public async Task GivenImplementation_WhenDeadLetterAsyncWithReasonIsCalled_ThenDeadLettersImplementation()
        {
            var lockToken = Utils.Random.GetString();
            var reason = Utils.Random.GetString();
            await _target.DeadLetterAsync(lockToken, reason);
            await _implementation.Received().DeadLetterAsync(lockToken, reason);
        }

        [Fact]
        public async Task GivenImplementation_WhenDeadLetterAsyncWithReasonAndDescriptionIsCalled_ThenDeadLettersImplementation()
        {
            var lockToken = Utils.Random.GetString();
            var reason = Utils.Random.GetString();
            var deadLetterErrorDescription = Utils.Random.GetString();
            await _target.DeadLetterAsync(lockToken, reason, deadLetterErrorDescription);
            await _implementation.Received().DeadLetterAsync(lockToken, reason, deadLetterErrorDescription);
        }

        [Fact]
        public async Task GivenImplementation_WhenCloseAsyncIsCalled_ThenClosesImplementation()
        {
            await _target.CloseAsync();
            await _implementation.Received().CloseAsync();
        }

        [Fact]
        public void GivenImplementation_WhenRegisterPluginIsCalled_ThenRegistersPluginImplementation()
        {
            var serviceBusPlugin = Substitute.For<ServiceBusPlugin>();
            _target.RegisterPlugin(serviceBusPlugin);
            _implementation.Received().RegisterPlugin(serviceBusPlugin);
        }

        [Fact]
        public void GivenImplementation_WhenUnregisterPluginIsCalled_ThenUnregistersPluginImplementation()
        {
            var serviceBusPluginName = Utils.Random.GetString();
            _target.UnregisterPlugin(serviceBusPluginName);
            _implementation.Received().UnregisterPlugin(serviceBusPluginName);
        }

        [Fact]
        public void GivenImplementation_WhenRegisterPluginsIsCalled_ThenRegistersPluginsImplementation()
        {
            var registeredPlugins = _target.RegisteredPlugins;
            _implementation.Received().RegisteredPlugins.ShouldBe(registeredPlugins);
        }
    }
}
