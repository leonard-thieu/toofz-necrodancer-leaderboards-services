using System;

namespace toofz.Services
{
    public interface IEnvironment
    {
        /// <summary>
        /// Gets or sets the fully qualified path of the current working directory.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Attempted to set to an empty string ("").
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Attempted to set to null.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurred.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// Attempted to set a local path that cannot be found.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the appropriate permission.
        /// </exception>
        string CurrentDirectory { get; set; }
        /// <summary>
        /// Gets a value indicating whether the current process is running in user interactive mode.
        /// </summary>
        bool UserInteractive { get; }
    }
}