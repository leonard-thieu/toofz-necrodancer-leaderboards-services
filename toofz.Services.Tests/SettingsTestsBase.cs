using System;
using System.Configuration;
using System.IO;
using Xunit;

namespace toofz.Services.Tests
{
    [Collection("Uses file system")]
    public abstract class SettingsTestsBase<TSettings> : IDisposable
        where TSettings : ApplicationSettingsBase
    {
        public SettingsTestsBase(TSettings settings)
        {
            File.Delete(ServiceSettingsProvider.ConfigFileName);
            this.settings = settings;
            settings.Reload();
        }

        internal readonly TSettings settings;

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            File.Delete(ServiceSettingsProvider.ConfigFileName);
        }

        #endregion
    }
}
