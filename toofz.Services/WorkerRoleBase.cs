using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace toofz.Services
{
    public abstract partial class WorkerRoleBase<TSettings> : ServiceBase
        where TSettings : ISettings
    {
        #region Static Members

        static readonly ILog Log = LogManager.GetLogger(typeof(WorkerRoleBase<TSettings>).GetSimpleFullName());

        static void LogError(string message, Exception ex)
        {
            var e = ex;
            if (ex is AggregateException)
            {
                var all = ((AggregateException)ex).Flatten();
                e = all.InnerExceptions.Count == 1 ? all.InnerException : all;
            }
            Log.Error(message, e);
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerRoleBase{TSettings}"/> class.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// The specified name is null or is longer than <see cref="ServiceBase.MaxNameLength"/>, 
        /// or the specified name contains forward slash or backslash characters.
        /// </exception>
        protected WorkerRoleBase(string serviceName)
        {
            ServiceName = serviceName;
        }

        #region Fields

        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        Thread thread;
        Idle idle;

        public abstract TSettings Settings { get; }

        #endregion

        /// <summary>
        /// Starts the update process. This method is intended to be called from console applications.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        public void ConsoleStart(params string[] args)
        {
            OnStart(args);
            thread.Join(Timeout.InfiniteTimeSpan);
        }

        #region OnStart

        /// <summary>
        /// Executes when a Start command is sent to the service by the Service Control Manager (SCM) 
        /// or when the operating system starts (for a service that starts automatically).
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            thread = new Thread(Run);
            thread.Start();
        }

        #endregion

        #region Run

        void Run()
        {
            var cancellationToken = cancellationTokenSource.Token;

            while (true)
            {
                try
                {
                    RunAsync(cancellationToken).Wait();
                }
                catch (Exception ex) when (!(ex.InnerException is TaskCanceledException))
                {
                    LogError("Failed to complete run due to an error.", ex);
                }

                try
                {
                    idle.DelayAsync(cancellationToken).Wait();
                }
                catch (TaskCanceledException)
                {
                    Log.Info("Received Stop command. Stopping service...");
                    return;
                }
            }
        }

        async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Settings.Reload();

                idle = Idle.StartNew(Settings.UpdateInterval);

                await RunAsyncOverride(cancellationToken).ConfigureAwait(false);

                GC.Collect();
                GC.WaitForPendingFinalizers();

                var remaining = idle.GetTimeRemaining();
                if (remaining > Settings.DelayBeforeGC)
                {
                    await Task.Delay(Settings.DelayBeforeGC, cancellationToken).ConfigureAwait(false);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                await idle.DelayAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// When overridden in a derived class, provides the main logic to run the update.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token that will be checked prior to completing the returned task.
        /// </param>
        /// <returns>A task that represents the update process.</returns>
        protected abstract Task RunAsyncOverride(CancellationToken cancellationToken);

        #endregion

        #region OnStop

        /// <summary>
        /// Executes when a Stop command is sent to the service by the Service Control Manager (SCM).
        /// </summary>
        protected override void OnStop()
        {
            cancellationTokenSource.Cancel();
            thread.Join(TimeSpan.FromSeconds(10));
        }

        #endregion
    }
}
