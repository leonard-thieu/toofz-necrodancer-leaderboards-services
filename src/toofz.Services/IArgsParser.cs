namespace toofz.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    public interface IArgsParser<TSettings>
        where TSettings : ISettings
    {
        /// <summary>
        /// Parses arguments into settings and saves them.
        /// </summary>
        /// <param name="args">The arguments to parse.</param>
        /// <param name="settings">The settings object.</param>
        /// <returns>
        /// Zero, if parsing was successful. Non-zero if there was an error while parsing.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="args"/> cannot be null.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="settings"/> cannot be null.
        /// </exception>
        int Parse(string[] args, TSettings settings);
    }
}