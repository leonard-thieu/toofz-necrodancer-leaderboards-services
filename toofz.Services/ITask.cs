using System;
using System.Threading;
using System.Threading.Tasks;

namespace toofz.Services
{
    interface ITask
    {
        /// <summary>
        /// Creates a cancellable task that completes after a specified time interval.
        /// </summary>
        /// <param name="delay">
        /// The time span to wait before completing the returned task, or TimeSpan.FromMilliseconds(-1) to wait indefinitely.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token that will be checked prior to completing the returned task.
        /// </param>
        /// <returns>A task that represents the time delay.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="delay"/> represents a negative time interval other than TimeSpan.FromMillseconds(-1).
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The delay argument's <see cref="TimeSpan.TotalMilliseconds"/> property is greater than <see cref="Int32.MaxValue"/>.
        /// </exception>
        /// <exception cref="TaskCanceledException">
        /// The task has been canceled.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// The provided <paramref name="cancellationToken"/> has already been disposed.
        /// </exception>
        Task Delay(TimeSpan delay, CancellationToken cancellationToken);
    }
}
