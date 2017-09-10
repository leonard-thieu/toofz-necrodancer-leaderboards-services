using System;
using System.Diagnostics.CodeAnalysis;

namespace toofz.Services
{
    [ExcludeFromCodeCoverage]
    sealed class ConsoleAdapter : IConsole
    {
        public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);
    }
}
