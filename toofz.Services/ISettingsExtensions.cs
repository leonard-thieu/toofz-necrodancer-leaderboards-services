namespace toofz.Services
{
    /// <summary>
    /// Contains extension methods for <see cref="ISettings"/>.
    /// </summary>
    internal static class ISettingsExtensions
    {
        /// <summary>
        /// Stores the current values of the application settings properties even if values have not been modified.
        /// </summary>
        /// <param name="settings">The settings to save.</param>
        /// <param name="force">
        /// If true, forces settings to persist even if values have not been modified.
        /// </param>
        public static void Save(this ISettings settings, bool force)
        {
            if (force)
            {
                // At least one property value must be dirty in order to have Save actually save.
                settings.KeyDerivationIterations = settings.KeyDerivationIterations;
            }

            settings.Save();
        }
    }
}
