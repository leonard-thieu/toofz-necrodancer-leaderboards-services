using System;
using System.Threading;
using System.Threading.Tasks;

namespace toofz.Services
{
    interface IIdle
    {
        /// <summary>
        /// Writes the time remaining until the start of the next cycle.
        /// </summary>
        void WriteTimeRemaining();
        /// <summary>
        /// Gets the time remaining until the start of the next cycle.
        /// </summary>
        /// <returns>
        /// The time remaining until the start of the next cycle.
        /// </returns>
        TimeSpan GetTimeRemaining();
        /// <summary>
        /// Creates a cancellable task that completes after the remaining time.
        /// </summary>
        /// <param name="cancellationToken">
        /// The cancellation token that will be checked prior to completing the returned task.
        /// </param>
        /// <returns>A task that represents the time delay.</returns>
        Task DelayAsync(CancellationToken cancellationToken);
    }
}