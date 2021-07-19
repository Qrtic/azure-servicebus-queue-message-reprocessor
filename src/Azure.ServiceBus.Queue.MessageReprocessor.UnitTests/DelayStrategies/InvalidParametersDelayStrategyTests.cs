using System;
using System.Collections.Generic;
using Azure.ServiceBus.Queue.MessageReprocessor.DelayStrategies;
using Shouldly;
using Xunit;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests.DelayStrategies
{
    public abstract class InvalidParametersDelayStrategyTests
    {
        protected abstract IRetryDelayStrategy GetStrategyWithMaxAttempts(int maxAttempts);

        [Theory]
        [InlineData(-100)]
        [InlineData(0)]
        public void GivenInvalidMaxAttempts_WhenConstructing_ThenThrows(int maxAttempts)
        {
            var exception = Should.Throw<ArgumentOutOfRangeException>(() => GetStrategyWithMaxAttempts(maxAttempts));

            exception.ParamName.ShouldBe("maxAttempts");
            exception.Message.ShouldStartWith("Should be greater or equal to 1.");
        }

        [Theory]
        [InlineData(-100)]
        [InlineData(0)]
        public void GivenInvalidAttempt_WhenCanDelayIsCalled_ThenThrows(int attempt)
        {
            var target = GetStrategyWithMaxAttempts(5);
            var exception = Should.Throw<ArgumentOutOfRangeException>(() => target.CanDelay(attempt));

            exception.ParamName.ShouldBe("attempt");
            exception.Message.ShouldStartWith("Should be greater or equal to 1.");
        }

        [Theory]
        [InlineData(-100)]
        [InlineData(0)]
        public void GivenInvalidAttempt_WhenGetDelayIsCalled_ThenThrows(int attempt)
        {
            var target = GetStrategyWithMaxAttempts(5);
            var exception = Should.Throw<ArgumentOutOfRangeException>(() => target.GetDelay(attempt));

            exception.ParamName.ShouldBe("attempt");
            exception.Message.ShouldStartWith("Should be greater or equal to 1.");
        }

        [Theory]
        [InlineData(3)]
        [InlineData(10)]
        public void GivenGreaterThanMaxAttemptsAttempt_WhenGetDelayIsCalled_ThenThrows(int attempt)
        {
            var maxAttempts = 2;
            var target = GetStrategyWithMaxAttempts(maxAttempts);
            var exception = Should.Throw<ArgumentOutOfRangeException>(() => target.GetDelay(attempt));

            exception.ParamName.ShouldBe("attempt");
            exception.Message.ShouldStartWith($"Should be less then maximum attempts '{maxAttempts}' count.");
        }

        public static IEnumerable<object[]> InvalidDelays() => new []
        {
            new object [] { TimeSpan.Parse("00:00:00") },
            new object [] { TimeSpan.Parse("-00:00:00.001") },
            new object [] { TimeSpan.Parse("-1.00:00:00") },
        };
    }

    public class InvalidParametersConstantDelayStrategyTests : InvalidParametersDelayStrategyTests
    {
        [Theory]
        [MemberData(nameof(InvalidDelays))]
        public void GivenInvalidDelay_WhenConstructing_ThenThrows(TimeSpan delay)
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                new ConstantDelayStrategy(1, delay));
        }

        protected override IRetryDelayStrategy GetStrategyWithMaxAttempts(int maxAttempts) =>
            new ConstantDelayStrategy(maxAttempts, TimeSpan.FromSeconds(1));
    }

    public class InvalidParametersLinearDelayStrategyTests : InvalidParametersDelayStrategyTests
    {
        [Theory]
        [MemberData(nameof(InvalidDelays))]
        public void GivenInvalidDelay_WhenConstructing_ThenThrows(TimeSpan delay)
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                new LinearDelayStrategy(1, delay, 1));
        }

        [Theory]
        [InlineData(-100)]
        [InlineData(0)]
        public void GivenInvalidFactor_WhenConstructing_ThenThrows(int factor)
        {
            var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
                new LinearDelayStrategy(1, TimeSpan.FromSeconds(1), factor));

            exception.ParamName.ShouldBe("factor");
            exception.Message.ShouldStartWith("Should be greater or equal to 1.");
        }

        protected override IRetryDelayStrategy GetStrategyWithMaxAttempts(int maxAttempts) =>
            new LinearDelayStrategy(maxAttempts, TimeSpan.FromSeconds(1), 1);
    }

    public class InvalidParametersExponentialDelayStrategyTests : InvalidParametersDelayStrategyTests
    {
        [Theory]
        [MemberData(nameof(InvalidDelays))]
        public void GivenInvalidDelay_WhenConstructing_ThenThrows(TimeSpan delay)
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                new ExponentialDelayStrategy(1, delay, 1));
        }

        [Theory]
        [InlineData(-100)]
        [InlineData(0)]
        public void GivenInvalidExponent_WhenConstructing_ThenThrows(int exponent)
        {
            var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
                new ExponentialDelayStrategy(1, TimeSpan.FromSeconds(1), exponent));

            exception.ParamName.ShouldBe("exponent");
            exception.Message.ShouldStartWith("Should be greater or equal to 1.");
        }

        protected override IRetryDelayStrategy GetStrategyWithMaxAttempts(int maxAttempts) =>
            new ExponentialDelayStrategy(maxAttempts, TimeSpan.FromSeconds(1), 1);
    }

    public class InvalidParametersExponentialWithJitterDelayStrategyTests : InvalidParametersDelayStrategyTests
    {
        [Theory]
        [MemberData(nameof(InvalidDelays))]
        public void GivenInvalidDelay_WhenConstructing_ThenThrows(TimeSpan delay)
        {
            Should.Throw<ArgumentOutOfRangeException>(() =>
                new ExponentialWithJitterDelayStrategy(1, delay, 1));
        }

        [Theory]
        [InlineData(-100)]
        [InlineData(0)]
        public void GivenInvalidExponent_WhenConstructing_ThenThrows(int exponent)
        {
            var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
                new ExponentialWithJitterDelayStrategy(1, TimeSpan.FromSeconds(1), exponent));

            exception.ParamName.ShouldBe("exponent");
            exception.Message.ShouldStartWith("Should be greater or equal to 1.");
        }

        protected override IRetryDelayStrategy GetStrategyWithMaxAttempts(int maxAttempts) =>
            new ExponentialWithJitterDelayStrategy(maxAttempts, TimeSpan.FromSeconds(1), 1);
    }
}
