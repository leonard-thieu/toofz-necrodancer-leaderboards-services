using System;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using log4net;

namespace toofz.Services
{
    public sealed class Application : IApplication
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(Application));

        public void Run<T, TSettings>()
            where T : WorkerRoleBase<TSettings>, new()
            where TSettings : ISettings
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Log.Fatal("Terminating application due to unhandled exception.", (Exception)e.ExceptionObject);
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Log.Fatal("Terminating application due to unobserved task exception.", e.Exception);
            };

            // Start as console application
            if (Environment.UserInteractive)
            {
                using (var worker = new T())
                {
                    worker.ConsoleStart();
                }
            }

            // Start as Windows service
            else
            {
                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                ServiceBase.Run(new T());
            }
        }
    }
}
