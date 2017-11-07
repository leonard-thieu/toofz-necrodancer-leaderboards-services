using System;
using System.Linq;

namespace toofz.Services
{
    internal sealed class ConsoleApplication<TSettings> : Application<TSettings>
        where TSettings : ISettings
    {
        private static bool IsCancelKeyPress(ConsoleKeyInfo keyInfo)
        {
            return
                (keyInfo.Modifiers == ConsoleModifiers.Control && keyInfo.Key == ConsoleKey.C) ||
                (keyInfo.Modifiers == ConsoleModifiers.Control && keyInfo.Key == ConsoleKey.Pause);
        }

        public ConsoleApplication(
            IConsoleWorkerRole worker,
            IArgsParser<TSettings> parser,
            IConsole console)
        {
            this.worker = worker ?? throw new ArgumentNullException(nameof(worker));
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
            this.console = console;
        }

        private readonly IConsoleWorkerRole worker;
        private readonly IArgsParser<TSettings> parser;
        private readonly IConsole console;

        internal override int RunOverride(string[] args, TSettings settings)
        {
            // Args are only allowed while running as a console application as they may require user input.
            if (args.Any())
            {
                return parser.Parse(args, settings);
            }

            worker.Start();

            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = console.ReadKey(intercept: true);
            } while (!IsCancelKeyPress(keyInfo));

            worker.Stop();

            return 0;
        }
    }
}
