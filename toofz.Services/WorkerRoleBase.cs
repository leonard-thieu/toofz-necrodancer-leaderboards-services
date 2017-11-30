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

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        protected WorkerRoleBase(string serviceName, TSettings settings, TelemetryClient telemetryClient) : this(serviceName, settings, telemetryClient, null) { }

        internal WorkerRoleBase(string serviceName, TSettings settings, TelemetryClient telemetryClient, ILog log)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            ServiceName = serviceName;
            Settings = settings;
            TelemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.log = log ?? Log;

            CanShutdown = true;
        }

        #region Fields

        private readonly ILog log;
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
        /// Signals that work has started.
        /// </summary>
        public Task Initialization => InitializationTcs.Task;

        private TaskCompletionSource<bool> InitializationTcs
        {
            get
            {
                lock (initializationTcsLock)
                {
                    return initializationTcs ?? (initializationTcs = new TaskCompletionSource<bool>());
                }
            }
            set
            {
                Debug.Assert(value == null, "Should only be set to null.");

                lock (initializationTcsLock)
                {
                    initializationTcs = value;
                }
            }
        }
        private TaskCompletionSource<bool> initializationTcs;
        private readonly object initializationTcsLock = new object();

        /// <summary>
        /// Signals that work has stopped due to a fault or <see cref="Stop"/> was called.
        /// </summary>
        public Task Completion { get; private set; }

        #endregion

        #region Start

        /// <summary>
        /// Starts the service. This method is intended to be called from console applications.
        /// </summary>
        public void Start()
        {
            OnStart(new string[0]);
        }

        /// <summary>
        /// Executes when a Start command is sent to the service by the Service Control Manager (SCM) 
        /// or when the operating system starts (for a service that starts automatically).
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            log.Info("Received Start command.");
            TelemetryClient.TrackEvent("Start command");

            cancellationTokenSource = new CancellationTokenSource();

            var completionCompletionSource = new TaskCompletionSource<bool>();
            // Completion should not end in a faulted state.
            // Proxy the Task that performs the work but don't marshal exceptions.
            Completion = completionCompletionSource.Task;

            var completion = RunAsync(cancellationTokenSource.Token);
            completion.ContinueWith(t =>
            {
                FlushTelemetry();
                completionCompletionSource.SetResult(true);
            }, TaskContinuationOptions.OnlyOnCanceled);
            completion.ContinueWith(async t =>
            {
                try
                {
                    await t.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    log.Error("Stopping service due to unhandled exception.", ex);
                    TelemetryClient.TrackException(ex);

                    FlushTelemetry();
                    cancellationTokenSource.Cancel();
                    completionCompletionSource.SetResult(true);
                    // Failure recovery options only trigger when terminating when not in the Stopped state.
                    // OnStop is called directly to avoid moving to the Stopped state.
                    OnStop();
                }
            }, TaskContinuationOptions.OnlyOnFaulted);

            InitializationTcs.SetResult(true);
        }

        #endregion

        #region Run

        internal async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await RunAsyncCore(Idle.StartNew(Settings.UpdateInterval), cancellationToken).ConfigureAwait(false);
            }
        }

        internal async Task RunAsyncCore(IIdle idle, CancellationToken cancellationToken)
        {
            Settings.Reload();

            await RunAsyncOverride(cancellationToken).ConfigureAwait(false);

            TelemetryClient.Flush();

            // Perform an eager garbage collection before entering an idle period. Otherwise, the GC performs garbage 
            // collection at the beginning of the next cycle and uses up resources when they're actually needed.
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

        #region Stop

        /// <summary>
        /// Executes when a Stop command is sent to the service by the Service Control Manager (SCM).
        /// </summary>
        protected override void OnStop()
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                log.Info("Received Stop command.");
                log.Info("Stopping service...");
                TelemetryClient.TrackEvent("Stop command");

                cancellationTokenSource.Cancel();
            }

            InitializationTcs = null;
            Completion.GetAwaiter().GetResult();

            log.Info("Stopped service.");
        }

        #endregion

        #region Shutdown

        /// <summary>
        /// When implemented in a derived class, executes when the system is shutting down. 
        /// Specifies what should occur immediately prior to the system shutting down.
        /// </summary>
        protected override void OnShutdown()
        {
            log.Info("Received Shutdown command.");
            TelemetryClient.TrackEvent("Shutdown command");

            cancellationTokenSource.Cancel();
            OnStop();
        }

        #endregion

        private void FlushTelemetry()
        {
            log.Info("Flushing telemetry...");

            // Flush runs asynchronously when using ServerTelemetryChannel. Waiting 2 seconds seems to be sufficient in 
            // order to get the majority of telemetry through.
            // https://github.com/Microsoft/ApplicationInsights-dotnet/issues/281
            TelemetryClient.Flush();
            Thread.Sleep(TimeSpan.FromSeconds(2));

            log.Info("Flushed telemetry (probably).");
        }
    }
}
