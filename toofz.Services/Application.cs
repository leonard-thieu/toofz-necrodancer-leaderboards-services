using System;
using System.IO;
using System.Threading.Tasks;
using log4net;
using Microsoft.ApplicationInsights.Extensibility;

namespace toofz.Services
{
    public abstract class Application<TSettings>
        where TSettings : ISettings
    {
        // Must receive log object from entry point so that logging is initialized properly.
        public static int Run<TWorkerRole>(
            string[] args,
            TSettings settings,
            TWorkerRole worker,
            IArgsParser<TSettings> parser,
            ILog log)
            where TWorkerRole : WorkerRoleBase<TSettings>
        {
            Application<TSettings> app;

            if (Environment.UserInteractive)
            {
                app = new ConsoleApplication<TSettings>(worker, parser, new ConsoleAdapter());
            }
            else
            {
                app = new ServiceApplication<TSettings>(worker, new ServiceBaseStaticAdapter());
            }

            return app.Run(args, settings, log, TelemetryConfiguration.Active);
        }

        internal int Run(
            string[] args,
            TSettings settings,
            ILog log,
            TelemetryConfiguration telemetryConfiguration)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            log.Debug("Initialized logging.");

            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                log.Fatal("Terminating application due to unhandled exception.", e.ExceptionObject as Exception);
            };
            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                log.Fatal("Terminating application due to unobserved task exception.", e.Exception);
            };

            // Services have their starting current directory set to the system directory. The current directory must 
            // be set to the base directory so the settings file may be found.
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            // Settings must be loaded before accessing them.
            settings.Reload();
            // Write default settings if settings file does not exist.
            settings.Save(force: true);

            if (string.IsNullOrEmpty(settings.InstrumentationKey) &&
                telemetryConfiguration.InstrumentationKey == "")
            {
                log.Warn("An Application Insights instrumentation key is not set. Telemetry will not be reported to Application Insights.");
            }
            else
            {
                telemetryConfiguration.InstrumentationKey = settings.InstrumentationKey;
            }

            return RunOverride(args, settings);
        }

        internal abstract int RunOverride(string[] args, TSettings settings);
    }
}
