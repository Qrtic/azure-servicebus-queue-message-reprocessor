using System;

namespace Azure.ServiceBus.Queue.MessageReprocessor.UnitTests.Extensions
{
    public static class RandomExtensions
    {
        public static TimeSpan GetTimeSpan(this Random random)
        {
            return TimeSpan.FromMilliseconds(random.Next());
        }

        public static string GetString(this Random _)
        {
            return Guid.NewGuid().ToString();
        }

        public static int GetInt(this Random random)
        {
            return random.Next();
        }

        public static T GetEnum<T>(this Random random) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(random.Next(0, values.Length));
        }
    }
}
