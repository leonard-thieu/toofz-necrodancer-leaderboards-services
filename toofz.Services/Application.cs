using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using log4net;
using Microsoft.ApplicationInsights.Extensibility;

namespace toofz.Services
{
    public static class Application
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(Application));

        [ExcludeFromCodeCoverage]
        public static int Run<TWorkerRole, TSettings>(
            string[] args,
            IEnvironment environment,
            TSettings settings,
            TWorkerRole worker,
            IArgsParser<TSettings> parser,
            IServiceBase serviceBase)
            where TWorkerRole : ServiceBase, IWorkerRole
            where TSettings : ISettings
        {
            return Run(args, environment, settings, worker, parser, serviceBase, Log, new ConsoleAdapter());
        }

        internal static int Run<TWorkerRole, TSettings>(
            string[] args,
            IEnvironment environment,
            TSettings settings,
            TWorkerRole worker,
            IArgsParser<TSettings> parser,
            IServiceBase serviceBase,
            ILog log,
            IConsole console)
            where TWorkerRole : ServiceBase, IWorkerRole
            where TSettings : ISettings
        {
            log.Debug("Initialized logging.");

            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                log.Fatal("Terminating application due to unhandled exception.", e.ExceptionObject as Exception);
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                log.Fatal("Terminating application due to unobserved task exception.", e.Exception);
            };

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            settings.Reload();

            // Args are only allowed while running as a console application as they may require user input.
            if (args.Any() && environment.UserInteractive)
            {
                if (parser == null)
                    throw new ArgumentNullException(nameof(parser));

                return parser.Parse(args, settings);
            }

            if (string.IsNullOrEmpty(settings.InstrumentationKey))
            {
                log.Warn("The setting 'InstrumentationKey' is not set. Telemetry is disabled.");
                TelemetryConfiguration.Active.DisableTelemetry = true;
            }
            else
            {
                TelemetryConfiguration.Active.InstrumentationKey = settings.InstrumentationKey;
                TelemetryConfiguration.Active.DisableTelemetry = false;
            }

            // Start as console application
            if (environment.UserInteractive)
            {
                worker.Start();

                ConsoleKeyInfo keyInfo;
                do
                {
                    keyInfo = console.ReadKey(intercept: true);
                } while (!IsCancelKeyPress(keyInfo));

                worker.Stop();
            }
            // Start as service
            else
            {
                if (serviceBase == null)
                    throw new ArgumentNullException(nameof(serviceBase));

                environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                serviceBase.Run(worker);
            }

            return 0;
        }

        static bool IsCancelKeyPress(ConsoleKeyInfo keyInfo)
        {
            return
                (keyInfo.Modifiers == ConsoleModifiers.Control && keyInfo.Key == ConsoleKey.C) ||
                (keyInfo.Modifiers == ConsoleModifiers.Control && keyInfo.Key == ConsoleKey.Pause);
        }
    }
}
