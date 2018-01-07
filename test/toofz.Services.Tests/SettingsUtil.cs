using System.Configuration;

namespace toofz.Services.Tests
{
    internal static class SettingsUtil
    {
        public static SettingsProperty CreateProperty<T>(string name)
        {
            return new SettingsProperty(name) { PropertyType = typeof(T) };
        }

        public static SettingsProperty CreateProperty<T>(string name, T defaultValue)
        {
            var property = CreateProperty<T>(name);
            property.DefaultValue = defaultValue;

            return property;
        }

        public static SettingsPropertyValue CreatePropertyValue<T>(string name)
        {
            var property = CreateProperty<T>(name);

            return new SettingsPropertyValue(property);
        }
    }
}
