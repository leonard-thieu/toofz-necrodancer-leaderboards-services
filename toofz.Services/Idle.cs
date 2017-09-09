using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace toofz.Services
{
    sealed class Idle : IIdle
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(Idle));

        public static Idle StartNew(TimeSpan updateInterval) => new Idle(updateInterval, DateTime.UtcNow, Log);

        internal Idle(TimeSpan updateInterval, DateTime startTime, ILog log)
        {
            this.updateInterval = updateInterval;
            this.startTime = startTime;
            this.log = log;
        }

        readonly TimeSpan updateInterval;
        readonly DateTime startTime;
        readonly ILog log;

        /// <summary>
        /// Writes the time remaining until the start of the next cycle.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void WriteTimeRemaining() => WriteTimeRemaining(DateTime.UtcNow);

        internal void WriteTimeRemaining(DateTime from)
        {
            var remaining = GetTimeRemaining(from);
            if (remaining > TimeSpan.Zero)
            {
                log.Info($"Next run takes place in {remaining.TotalSeconds:F0} seconds...");
            }
            else
            {
                log.Info("Next run starting immediately...");
            }
        }

        /// <summary>
        /// Gets the time remaining until the start of the next cycle.
        /// </summary>
        /// <returns>
        /// The time remaining until the start of the next cycle.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public TimeSpan GetTimeRemaining() => GetTimeRemaining(DateTime.UtcNow);

        internal TimeSpan GetTimeRemaining(DateTime from) => updateInterval - (from - startTime);

        /// <summary>
        /// Creates a cancellable task that completes after the remaining time.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token that will be checked prior to completing the returned task.
        /// </param>
        /// <returns>A task that represents the time delay.</returns>
        [ExcludeFromCodeCoverage]
        public Task DelayAsync(CancellationToken cancellationToken) => DelayAsync(DateTime.UtcNow, new TaskAdapter(), cancellationToken);

        internal async Task DelayAsync(DateTime now, ITask task, CancellationToken cancellationToken)
        {
            var remaining = GetTimeRemaining(now);
            if (remaining > TimeSpan.Zero)
            {
                await task.Delay(remaining, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
