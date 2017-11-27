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
        /// <summary>
        /// Initializes the application and begins executing update cycles. This should be called on application startup.
        /// </summary>
        /// <typeparam name="TWorkerRole">The type of the worker that extends from <see cref="WorkerRoleBase{TSettings}"/>.</typeparam>
        /// <param name="args">Arguments passed in from the command line.</param>
        /// <param name="settings">The settings object.</param>
        /// <param name="worker">The worker object.</param>
        /// <param name="parser">The parser object.</param>
        /// <param name="log">The logging object to use for initializing logging.</param>
        /// <returns>
        /// 0 - The application ran successfully.
        /// 1 - There was an error parsing <paramref name="args"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="args"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settings"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="worker"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="parser"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="log"/> is null.
        /// </exception>
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
                app = new ConsoleApplication<TSettings>(worker, parser, new ConsoleStaticAdapter());
            }
            else
            {
                app = new ServiceApplication<TSettings>(worker, new ServiceBaseStaticAdapter());
            }

            return app.RunAsync(args, settings, log, TelemetryConfiguration.Active).GetAwaiter().GetResult();
        }

        internal Application() { }

        internal Task<int> RunAsync(
            string[] args,
            TSettings settings,
            ILog log,
            TelemetryConfiguration telemetryConfiguration)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            // Must receive log object from entry point so that logging is initialized properly.
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

            return RunAsyncOverride(args, settings);
        }

        internal abstract Task<int> RunAsyncOverride(string[] args, TSettings settings);
    }
}
