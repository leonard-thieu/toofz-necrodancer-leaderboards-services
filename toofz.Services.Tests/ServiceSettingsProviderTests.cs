using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using toofz.Services.Tests.Properties;
using Xunit;

namespace toofz.Services.Tests
{
    public class ServiceSettingsProviderTests
    {
        public class ApplicationName
        {
            [Fact]
            public void SetToNull_ThrowsArgumentNullException()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                string applicationName = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    provider.ApplicationName = applicationName;
                });
            }

            [Fact]
            public void ReturnsADefaultValue()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();

                // Act
                var applicationName = provider.ApplicationName;

                // Assert
                Assert.Equal("toofz.Services", applicationName);
            }

            [Fact]
            public void GetSetBehavior()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                string applicationName = "My Application";

                // Act
                provider.ApplicationName = applicationName;

                // Assert
                Assert.Equal(applicationName, provider.ApplicationName);
            }
        }

        public class GetSettingsReader
        {
            [Fact]
            public void SetToNull_ThrowsArgumentNullException()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                Func<TextReader> getSettingsReader = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    provider.GetSettingsReader = getSettingsReader;
                });
            }

            [Fact]
            public void ReturnsADefaultValue()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();

                // Act
                var getSettingsReader = provider.GetSettingsReader;

                // Assert
                Assert.NotNull(getSettingsReader);
            }

            [Fact]
            public void GetSetBehavior()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                Func<TextReader> getSettingsReader = () => new StringReader("");

                // Act
                provider.GetSettingsReader = getSettingsReader;

                // Assert
                Assert.Equal(getSettingsReader, provider.GetSettingsReader);
            }
        }

        public class GetSettingsWriter
        {
            [Fact]
            public void SetToNull_ThrowsArgumentNullException()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                Func<TextWriter> getSettingsWriter = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    provider.GetSettingsWriter = getSettingsWriter;
                });
            }

            [Fact]
            public void ReturnsADefaultValue()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();

                // Act
                var getSettingsWriter = provider.GetSettingsWriter;

                // Assert
                Assert.NotNull(getSettingsWriter);
            }

            [Fact]
            public void GetSetBehavior()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                Func<TextWriter> getSettingsWriter = () => new StringWriter();

                // Act
                provider.GetSettingsWriter = getSettingsWriter;

                // Assert
                Assert.Equal(getSettingsWriter, provider.GetSettingsWriter);
            }
        }

        public class Initialize
        {
            [Fact]
            public void NameIsNull_DoesNotThrowArgumentNullException()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();

                // Act -> Assert
                provider.Initialize(null, new NameValueCollection());
            }

            [Fact]
            public void Initializes()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();

                // Act
                provider.Initialize("myName", new NameValueCollection());

                // Assert
                Assert.Equal(provider.ApplicationName, provider.Name);
                Assert.Equal(provider.ApplicationName, provider.Description);
            }
        }

        public class GetPropertyValues
        {
            [Fact]
            public void NoConfig_ReturnsDefaultValues()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                provider.GetSettingsReader = () => new StringReader("");
                var context = new SettingsContext();
                var properties = new SettingsPropertyCollection();
                var property1 = SettingsUtil.CreateProperty("myProp1", "myDefaultValue1");
                properties.Add(property1);
                var property2 = SettingsUtil.CreateProperty("myProp2", "myDefaultValue2");
                properties.Add(property2);

                // Act
                var values = provider.GetPropertyValues(context, properties);

                // Assert
                Assert.Equal(2, values.Count);
                Assert.Equal("myDefaultValue1", values["myProp1"].PropertyValue);
                Assert.Equal("myDefaultValue2", values["myProp2"].PropertyValue);
            }

            [Fact]
            public void HandlesSerializeAsXml()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                provider.GetSettingsReader = () => new StringReader(Resources.SerializeAsXmlConfig);
                var context = new SettingsContext();
                var properties = new SettingsPropertyCollection();
                var property = SettingsUtil.CreateProperty<XmlSerializable>("myProp");
                property.SerializeAs = SettingsSerializeAs.Xml;
                properties.Add(property);

                // Act
                var values = provider.GetPropertyValues(context, properties);
                var myProp = values["myProp"].PropertyValue;

                // Assert
                Assert.IsAssignableFrom<XmlSerializable>(myProp);
            }

            [Fact]
            public void ReturnsValuesFromConfig()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                provider.GetSettingsReader = () => new StringReader(Resources.BasicConfig);
                var context = new SettingsContext();
                var properties = new SettingsPropertyCollection();
                var property1 = SettingsUtil.CreateProperty<string>("myProp1");
                properties.Add(property1);
                var property2 = SettingsUtil.CreateProperty<string>("myProp2");
                properties.Add(property2);

                // Act
                var values = provider.GetPropertyValues(context, properties);

                // Assert
                Assert.Equal(2, values.Count);
                Assert.Equal("mySerializedValue1", values["myProp1"].PropertyValue);
                Assert.Equal("mySerializedValue2", values["myProp2"].PropertyValue);
            }
        }

        public class SetPropertyValues
        {
            [Fact]
            public void SetsValuesInConfig()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                var sw = new StringWriter();
                provider.GetSettingsWriter = () => sw;
                var context = new SettingsContext();
                var values = new SettingsPropertyValueCollection();
                var property1 = new SettingsProperty("myProp1");
                var value1 = new SettingsPropertyValue(property1) { SerializedValue = "mySerializedValue1" };
                values.Add(value1);
                var property2 = new SettingsProperty("myProp2");
                var value2 = new SettingsPropertyValue(property2) { SerializedValue = "mySerializedValue2" };
                values.Add(value2);

                // Act
                provider.SetPropertyValues(context, values);

                // Assert
                Assert.Equal(Resources.BasicConfig, sw.ToString());
            }

            [Fact]
            public void HandlesSerializeAsXml()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                var sw = new StringWriter();
                provider.GetSettingsWriter = () => sw;
                var context = new SettingsContext();
                var values = new SettingsPropertyValueCollection();
                var value = SettingsUtil.CreatePropertyValue<XmlSerializable>("myProp");
                value.Property.SerializeAs = SettingsSerializeAs.Xml;
                value.PropertyValue = new XmlSerializable
                {
                    Name = "My Serializable Type",
                    Number = 22,
                    Data = new byte[] { 1, 2, 3, 4 },
                };
                values.Add(value);

                // Act
                provider.SetPropertyValues(context, values);

                // Assert
                Assert.Equal(Resources.SerializeAsXmlConfig, sw.ToString(), ignoreLineEndingDifferences: true);
            }

            [Fact]
            public void SerializesTimeSpanInHumanReadableFormat()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                var sw = new StringWriter();
                provider.GetSettingsWriter = () => sw;
                var context = new SettingsContext();
                var values = new SettingsPropertyValueCollection();
                var value = SettingsUtil.CreatePropertyValue<TimeSpan>("myProp");
                value.PropertyValue = TimeSpan.Zero;
                values.Add(value);

                // Act
                provider.SetPropertyValues(context, values);

                // Assert
                Assert.Equal(Resources.TimeSpanConfig, sw.ToString(), ignoreLineEndingDifferences: true);
            }
        }

        [Trait("Category", "Uses Settings")]
        [Collection(SettingsCollection.Name)]
        public class IntegrationTests
        {
            public IntegrationTests(SettingsFixture settingsFixture)
            {
                settings = ServiceSettingsProviderSettings.Default;
                settings.Reload();
            }

            private ServiceSettingsProviderSettings settings;

            [Fact]
            public void ReturnsDefaultValueIfValueIsNotPresent()
            {
                // Arrange -> Act
                var appId = settings.AppId;

                // Assert
                Assert.Equal(247080U, appId);
            }

            [Fact]
            public void SavesChangedSetting()
            {
                // Arrange
                settings.ForceSave = true;

                // Act
                settings.Save();
                settings.Reload();
                var appId = settings.AppId;

                // Assert
                Assert.True(settings.ForceSave);
            }

            [Fact]
            public void PersistsDefaultValue()
            {
                // Arrange
                settings.ForceSave = true;

                // Act
                settings.Save();

                // Assert
                var doc = XDocument.Load(ServiceSettingsProvider.ConfigFileName);
                var appIdEl = (from s in doc.Descendants("setting")
                               where s.Attributes("name").Single().Value == "AppId"
                               select s.Element("value"))
                              .Single();
                Assert.Equal(247080.ToString(), appIdEl.Value);
            }

            [Fact]
            public void PersistsDefaultTimeSpanInHumanReadableFormat()
            {
                // Arrange
                settings.ForceSave = true;

                // Act
                settings.Save();

                // Assert
                var doc = XDocument.Load(ServiceSettingsProvider.ConfigFileName);
                var durationEl = (from s in doc.Descendants("setting")
                                  where s.Attributes("name").Single().Value == "Duration"
                                  select s.Element("value"))
                                 .Single();
                Assert.Equal("00:02:00", durationEl.Value);
            }

            [Fact]
            public void PersistsSpecifiedTimeSpanInHumanReadableFormat()
            {
                // Arrange
                settings.Duration = TimeSpan.FromSeconds(234);

                // Act
                settings.Save();

                // Assert
                var doc = XDocument.Load(ServiceSettingsProvider.ConfigFileName);
                var durationEl = (from s in doc.Descendants("setting")
                                  where s.Attributes("name").Single().Value == "Duration"
                                  select s.Element("value"))
                                 .Single();
                Assert.Equal("00:03:54", durationEl.Value);
            }

            [Fact]
            public void ReadsTimeSpanInHumanReadableFormat()
            {
                // Arrange
                settings.Duration = TimeSpan.FromSeconds(234);
                settings.Save();

                // Act
                settings.Reload();
                var duration = settings.Duration;

                // Assert
                Assert.Equal(TimeSpan.FromSeconds(234), duration);
            }
        }
    }
}
