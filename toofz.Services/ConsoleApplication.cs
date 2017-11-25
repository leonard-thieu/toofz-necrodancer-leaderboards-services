using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace toofz.Services
{
    internal sealed class ConsoleApplication<TSettings> : Application<TSettings>
        where TSettings : ISettings
    {
        private static bool IsCancelKeyPress(ConsoleKeyInfo keyInfo)
        {
            // Visual Studio's debugger traps Control-C and Control-Break by default.
            return Debugger.IsAttached ?
                keyInfo.Key == ConsoleKey.Enter :
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

        internal override async Task<int> RunAsyncOverride(string[] args, TSettings settings)
        {
            // Args are only allowed while running as a console application as they may require user input.
            if (args.Any())
            {
                return parser.Parse(args, settings);
            }

            worker.Start();

            // Watching on a separate thread so that exceptions can be observed.
            var cancelKeyPressWatcher = new Thread(StopOnCancelKeyPress) { IsBackground = true };
            cancelKeyPressWatcher.Start();

            await worker.Initialization.ConfigureAwait(false);
            await worker.Completion.ConfigureAwait(false);

            return 0;
        }

        private void StopOnCancelKeyPress()
        {
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = console.ReadKey(intercept: true);
            } while (!IsCancelKeyPress(keyInfo));

            worker.Stop();
        }
    }
}
