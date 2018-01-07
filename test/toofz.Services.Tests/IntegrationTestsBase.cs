using System;
using System.Configuration;
using System.IO;
using Xunit;

namespace toofz.Services.Tests
{
    [Trait("Category", "Uses file system")]
    [Collection("Uses file system")]
    public abstract class IntegrationTestsBase<TSettings> : IDisposable
        where TSettings : ApplicationSettingsBase
    {
        public IntegrationTestsBase(TSettings settings)
        {
            originalCurrentDirectory = Directory.GetCurrentDirectory();
            // Services start with their current directory set to the system directory.
            Directory.SetCurrentDirectory(Environment.SystemDirectory);

            File.Delete(ServiceSettingsProvider.ConfigFileName);
            this.settings = settings;
            settings.Reload();
        }

        private readonly string originalCurrentDirectory;
        internal readonly TSettings settings;

        #region IDisposable Members

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) { return; }

            if (disposing)
            {
                File.Delete(ServiceSettingsProvider.ConfigFileName);

                Directory.SetCurrentDirectory(originalCurrentDirectory);
            }
            disposed = true;
        }

        #endregion
    }
}
