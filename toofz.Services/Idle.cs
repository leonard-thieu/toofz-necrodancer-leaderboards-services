using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace toofz.Services
{
    sealed class Idle
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(Idle));

        public static Idle StartNew(TimeSpan updateInterval)
        {
            return new Idle(updateInterval, DateTime.UtcNow);
        }

        static void LogTimeRemaining(TimeSpan remaining)
        {
            if (remaining > TimeSpan.Zero)
            {
                Log.Info($"Next run takes place in {remaining.TotalSeconds:F0} seconds...");
            }
            else
            {
                Log.Info("Next run starting immediately...");
            }
        }

        Idle(TimeSpan updateInterval, DateTime startTime)
        {
            this.updateInterval = updateInterval;
            this.startTime = startTime;
        }

        readonly TimeSpan updateInterval;
        readonly DateTime startTime;

        public TimeSpan GetTimeRemaining()
        {
            return updateInterval - (DateTime.UtcNow - startTime);
        }

        public async Task DelayAsync(CancellationToken cancellationToken)
        {
            var remaining = GetTimeRemaining();
            LogTimeRemaining(remaining);
            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
