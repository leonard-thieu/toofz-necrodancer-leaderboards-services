using System.Configuration;

namespace toofz.Services.Tests.Properties
{
    [SettingsProvider(typeof(ServiceSettingsProvider))]
    partial class TestSettings : ISettings { }
}