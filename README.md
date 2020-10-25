![Build](https://github.com/Qrtic/azure-servicebus-queue-message-reprocessor/workflows/Build/badge.svg) [![codecov](https://codecov.io/gh/Qrtic/azure-servicebus-queue-message-reprocessor/branch/main/graph/badge.svg?token=2RMSWMKG98)](azure-servicebus-queue-message-reprocessor)

# azure-servicebus-queue-message-reprocessor
This .NET Standard library adds a wrapper for Azure Service Bus Queue client to customize message handling retry mechanism.

## When it might be useful
1. Scenarios that use an external service, which might be down or fails to handle the incoming request.
2. Scenarios when default retry processing mechanism is not suitable or desirable.
3. Scenarios with customizable retry delay.

## Requires
1. Connection string to the Azure Service Bus should have Listen-and-Write access role for an existing Queue.
2. Or connection string to the Azure Service Bus should have Manage access role (to be able to create a Queue dynamically).

## API (draft)

```C#
IQueueClient queueClient = new RetryableQueueClient(...);

queueClient.RegisterMessageHandler(OnMessageReceived, new RetryOptions());

void onMessageReceived(Message message)
{
  HttpResponseMessage response = _externalService.Call(new Request(message));
  if (response.StatusCode == HttpStatusCode.InternalServerError)
  {
    throw new RetryableOperationException();
  }
}
```

## Maintainer(s)

- [@qrtic](https://github.com/qrtic)