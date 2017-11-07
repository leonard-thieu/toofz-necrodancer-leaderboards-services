using System;
using System.IO;
using Xunit;

namespace toofz.Services.Tests
{
    public sealed class SettingsFixture : IDisposable
    {
        public SettingsFixture()
        {
            File.Delete(ServiceSettingsProvider.ConfigFileName);
        }

        public void Dispose()
        {
            File.Delete(ServiceSettingsProvider.ConfigFileName);
        }
    }

    [CollectionDefinition(Name)]
    public sealed class SettingsCollection : ICollectionFixture<SettingsFixture>
    {
        public const string Name = "Settings collection";
    }
}
