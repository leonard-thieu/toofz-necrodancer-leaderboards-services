using System;

namespace toofz.Services
{
    internal sealed class ConsoleAdapter : IConsole
    {
        public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);
    }
}
