using System;
using Moq;

namespace toofz.Services.Tests
{
    internal static class LogUtil
    {
        public static Func<string> IsMessage(string message)
        {
            return It.Is<Func<string>>(v => Matches(v, message));
        }

        private static bool Matches(Func<string> v, string message)
        {
            if (v == null) { return false; }

            return v() == message;
        }

        public static Func<string> IsAnyMessage()
        {
            return It.Is<Func<string>>(v => Matches(v));
        }

        private static bool Matches(Func<string> v)
        {
            if (v == null) { return false; }

            return v() != null;
        }
    }
}
