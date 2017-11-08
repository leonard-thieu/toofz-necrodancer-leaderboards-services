using System;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using log4net;

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
            where TWorkerRole : ServiceBase, IConsoleWorkerRole
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

            return app.Run(args, settings, log);
        }

        internal int Run(
            string[] args,
            TSettings settings,
            ILog log)
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

            // Settings must be loaded before accessing them.
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            settings.Reload();

            return RunOverride(args, settings);
        }

        internal abstract int RunOverride(string[] args, TSettings settings);
    }
}
