using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Polly;
using Polly.Utilities;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    public static class RetryableQueue
    {
        public static void RegisterMessageHandler(IQueueClient queueClient,
            Func<Message, CancellationToken, Task> handler,
            IAsyncPolicy retryPolicy)
        {
            var retryableMessageHandler = new RetryableMessageHandler(
                queueClient,
                handler,
                retryPolicy);
            queueClient.RegisterMessageHandler(
                retryableMessageHandler.Handler,
                retryableMessageHandler.ExceptionReceivedHandler);
        }

        class RetryableMessageHandler
        {
            private IQueueClient queueClient;
            private Func<Message, CancellationToken, Task> handler;
            private IAsyncPolicy retryPolicy;

            public RetryableMessageHandler(IQueueClient queueClient,
                Func<Message, CancellationToken, Task> handler,
                IAsyncPolicy retryPolicy)
            {
                this.queueClient = queueClient;
                this.handler = handler;
                this.retryPolicy = retryPolicy;
            }

            public Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
            {
                return Task.CompletedTask;
            }

            public async Task Handler(Message arg1, CancellationToken arg2)
            {
                var context = new Context("");

                await handler(arg1, arg2);
            }
        }
    }


    internal static class QueueRetryEngine
    {
        internal static async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action,
            Context context,
            CancellationToken cancellationToken,
            ExceptionPredicates shouldRetryExceptionPredicates,
            ResultPredicates<TResult> shouldRetryResultPredicates,
            Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync,
            int permittedRetryCount = int.MaxValue,
            IEnumerable<TimeSpan> sleepDurationsEnumerable = null,
            Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider = null,
            bool continueOnCapturedContext = false)
        {
            int tryCount = 0;
            IEnumerator<TimeSpan> sleepDurationsEnumerator = sleepDurationsEnumerable?.GetEnumerator();

            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    bool canRetry;
                    DelegateResult<TResult> outcome;

                    try
                    {
                        TResult result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

                        if (!shouldRetryResultPredicates.AnyMatch(result))
                        {
                            return result;
                        }

                        canRetry = tryCount < permittedRetryCount && (sleepDurationsEnumerable == null || sleepDurationsEnumerator.MoveNext());

                        if (!canRetry)
                        {
                            return result;
                        }

                        outcome = new DelegateResult<TResult>(result);
                    }
                    catch (Exception ex)
                    {
                        Exception handledException = shouldRetryExceptionPredicates.FirstMatchOrDefault(ex);
                        if (handledException == null)
                        {
                            throw;
                        }

                        canRetry = tryCount < permittedRetryCount && (sleepDurationsEnumerable == null || sleepDurationsEnumerator.MoveNext());

                        if (!canRetry)
                        {
                            handledException.RethrowWithOriginalStackTraceIfDiffersFrom(ex);
                            throw;
                        }

                        outcome = new DelegateResult<TResult>(handledException);
                    }

                    if (tryCount < int.MaxValue) { tryCount++; }

                    TimeSpan waitDuration = sleepDurationsEnumerator?.Current ?? (sleepDurationProvider?.Invoke(tryCount, outcome, context) ?? TimeSpan.Zero);

                    await onRetryAsync(outcome, waitDuration, tryCount, context).ConfigureAwait(continueOnCapturedContext);

                    if (waitDuration > TimeSpan.Zero)
                    {
                        await SystemClock.SleepAsync(waitDuration, cancellationToken).ConfigureAwait(continueOnCapturedContext);
                    }
                }
            }
            finally
            {
                sleepDurationsEnumerator?.Dispose();
            }
        }
    }
}
