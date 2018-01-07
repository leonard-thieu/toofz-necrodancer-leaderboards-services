using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace toofz.Services
{
    /// <summary>
    /// Wraps the static members on <see cref="Directory"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class DirectoryStaticAdapter : IDirectoryStatic
    {
        /// <summary>
        /// Sets the application's current working directory to the specified directory.
        /// </summary>
        /// <param name="path">The path to which the current working directory is set.</param>
        /// <exception cref="IOException">
        /// An I/O error occurred.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="path"/> is a zero-length string, contains only white space, or contains one or more 
        /// invalid characters. You can query for invalid characters with the <see cref="Path.GetInvalidPathChars()"/> 
        /// method.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="path"/> is null.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length. 
        /// For example, on Windows-based platforms, paths must be less than 248 characters 
        /// and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission to access unmanaged code.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// The specified path was not found.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The specified directory was not found.
        /// </exception>
        public void SetCurrentDirectory(string path) => Directory.SetCurrentDirectory(path);
    }
}
