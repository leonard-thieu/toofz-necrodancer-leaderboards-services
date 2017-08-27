using System;
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

        public static int Run<TWorkerRole, TSettings>(
            string[] args,
            IEnvironment environment,
            TSettings settings,
            TWorkerRole worker,
            IArgsParser<TSettings> parser,
            IServiceBase serviceBase,
            ILog log)
            where TWorkerRole : ServiceBase, IWorkerRole
            where TSettings : ISettings
        {
            log = log ?? Log;
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

            // Args are only allowed while running as a console as they may require user input.
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
                worker.ConsoleStart();
            }
            // Start as Windows service
            else
            {
                if (serviceBase == null)
                    throw new ArgumentNullException(nameof(serviceBase));

                environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                serviceBase.Run(worker);
            }

            return 0;
        }
    }
}
