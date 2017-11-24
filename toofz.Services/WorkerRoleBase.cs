using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.ApplicationInsights;

namespace toofz.Services
{
    public abstract class WorkerRoleBase<TSettings> : ServiceBase, IConsoleWorkerRole
        where TSettings : ISettings
    {
        #region Static Members

        private static readonly ILog Log = LogManager.GetLogger(typeof(WorkerRoleBase<TSettings>).GetSimpleFullName());

        [Conditional("FEATURE_GC_ENDOFCYCLE")]
        private static void GCCollect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerRoleBase{TSettings}"/> class.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// The specified name is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The specified name is longer than <see cref="ServiceBase.MaxNameLength"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The specified name contains forward slash characters.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The specified name contains backslash characters.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settings"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="telemetryClient"/> is null.
        /// </exception>
        protected WorkerRoleBase(string serviceName, TSettings settings, TelemetryClient telemetryClient)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            ServiceName = serviceName;
            Settings = settings;
            TelemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));

            CanShutdown = true;
        }

        #region Fields

        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Gets the settings object.
        /// </summary>
        protected TSettings Settings { get; }
        /// <summary>
        /// Get the <see cref="Microsoft.ApplicationInsights.TelemetryClient"/> to track telemetry.
        /// </summary>
        protected TelemetryClient TelemetryClient { get; }
        /// <summary>
        /// Signals that work has stopped due to a fault or the <see cref="ServiceBase.Stop"/> command was issued.
        /// </summary>
        public Task Completion { get; private set; }

        #endregion

        /// <summary>
        /// Starts the service. This method is intended to be called from console applications.
        /// </summary>
        /// <param name="args">Data passed by the command line.</param>
        public void Start(params string[] args)
        {
            OnStart(args);
        }

        #region OnStart

        /// <summary>
        /// Executes when a Start command is sent to the service by the Service Control Manager (SCM) 
        /// or when the operating system starts (for a service that starts automatically).
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            TelemetryClient.TrackEvent("Start service");
            cancellationTokenSource = new CancellationTokenSource();
            Completion = RunAsync(Log, cancellationTokenSource.Token);
            Completion.ContinueWith(t =>
            {
                Stop();
            }, TaskContinuationOptions.OnlyOnFaulted);
            Completion.ContinueWith(t =>
            {
                Log.Info("Stopped service.");
                cancellationTokenSource.Dispose();

                // Flush runs asynchronously when using ServerTelemetryChannel. Waiting 2 seconds seems to be sufficient in 
                // order to get the majority of telemetry through.
                // https://github.com/Microsoft/ApplicationInsights-dotnet/issues/281
                TelemetryClient.Flush();
                Thread.Sleep(TimeSpan.FromSeconds(2));
            });
        }

        #endregion

        #region Run

        internal async Task RunAsync(ILog log, CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    await RunAsyncCore(Idle.StartNew(Settings.UpdateInterval), log, cancellationToken).ConfigureAwait(false);
                }
                // Stop was called.
                catch (TaskCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        internal async Task RunAsyncCore(IIdle idle, ILog log, CancellationToken cancellationToken)
        {
            Settings.Reload();

            await RunAsyncOverride(cancellationToken).ConfigureAwait(false);

            TelemetryClient.Flush();

            GCCollect();

            idle.WriteTimeRemaining();

            var remaining = idle.GetTimeRemaining();
            if (remaining > Settings.DelayBeforeGC)
            {
                await Task.Delay(Settings.DelayBeforeGC, cancellationToken).ConfigureAwait(false);
                GCCollect();
            }

            await idle.DelayAsync(cancellationToken).ConfigureAwait(false);
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
            TelemetryClient.TrackEvent("Stop service");
            Log.Info("Stopping service...");
            cancellationTokenSource.Cancel();
            Completion.GetAwaiter().GetResult();
        }

        #endregion

        /// <summary>
        /// When implemented in a derived class, executes when the system is shutting down. 
        /// Specifies what should occur immediately prior to the system shutting down.
        /// </summary>
        protected override void OnShutdown()
        {
            TelemetryClient.TrackEvent("Shutdown service");
            Stop();
        }
    }
}
