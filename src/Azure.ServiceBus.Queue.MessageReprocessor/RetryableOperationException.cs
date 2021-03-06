﻿#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Azure.ServiceBus.Queue.MessageReprocessor
{
    [ExcludeFromCodeCoverage]
    public class RetryableOperationException : Exception
    {
        public RetryableOperationException()
        {
        }

        protected RetryableOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RetryableOperationException(string? message) : base(message)
        {
        }

        public RetryableOperationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
