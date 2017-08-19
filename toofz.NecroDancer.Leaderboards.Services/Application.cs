using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;
using log4net;

namespace toofz.NecroDancer.Leaderboards.Services
{
    public static class Application
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(Application));

        public static void Run<T>()
            where T : WorkerRoleBase, new()
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
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ServiceBase.Run(new T());
            }
        }
    }
}
