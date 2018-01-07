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
        private readonly ServiceSettingsProvider provider = new ServiceSettingsProvider();

        public class ApplicationNameProperty : ServiceSettingsProviderTests
        {
            [DisplayFact(nameof(ArgumentNullException))]
            public void SetToNull_ThrowsArgumentNullException()
            {
                // Arrange
                string applicationName = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    provider.ApplicationName = applicationName;
                });
            }

            [DisplayFact]
            public void ReturnsADefaultValue()
            {
                // Arrange -> Act
                var applicationName = provider.ApplicationName;

                // Assert
                Assert.Equal("toofz.Services", applicationName);
            }

            [DisplayFact]
            public void GetSetBehavior()
            {
                // Arrange
                string applicationName = "My Application";

                // Act
                provider.ApplicationName = applicationName;

                // Assert
                Assert.Equal(applicationName, provider.ApplicationName);
            }
        }

        public class GetSettingsReaderProperty : ServiceSettingsProviderTests
        {
            [DisplayFact(nameof(ArgumentNullException))]
            public void SetToNull_ThrowsArgumentNullException()
            {
                // Arrange
                Func<TextReader> getSettingsReader = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    provider.GetSettingsReader = getSettingsReader;
                });
            }

            [DisplayFact]
            public void ReturnsADefaultValue()
            {
                // Arrange -> Act
                var getSettingsReader = provider.GetSettingsReader;

                // Assert
                Assert.NotNull(getSettingsReader);
            }

            [DisplayFact]
            public void GetSetBehavior()
            {
                // Arrange
                Func<TextReader> getSettingsReader = () => new StringReader("");

                // Act
                provider.GetSettingsReader = getSettingsReader;

                // Assert
                Assert.Equal(getSettingsReader, provider.GetSettingsReader);
            }
        }

        public class GetSettingsWriterProperty : ServiceSettingsProviderTests
        {
            [DisplayFact(nameof(ArgumentNullException))]
            public void SetToNull_ThrowsArgumentNullException()
            {
                // Arrange
                Func<TextWriter> getSettingsWriter = null;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    provider.GetSettingsWriter = getSettingsWriter;
                });
            }

            [DisplayFact]
            public void ReturnsADefaultValue()
            {
                // Arrange -> Act
                var getSettingsWriter = provider.GetSettingsWriter;

                // Assert
                Assert.NotNull(getSettingsWriter);
            }

            [DisplayFact]
            public void GetSetBehavior()
            {
                // Arrange
                Func<TextWriter> getSettingsWriter = () => new StringWriter();

                // Act
                provider.GetSettingsWriter = getSettingsWriter;

                // Assert
                Assert.Equal(getSettingsWriter, provider.GetSettingsWriter);
            }
        }

        public class InitializeMethod : ServiceSettingsProviderTests
        {
            private string name = "myName";
            private NameValueCollection config = new NameValueCollection();

            [DisplayFact(nameof(ArgumentNullException))]
            public void NameIsNull_DoesNotThrowArgumentNullException()
            {
                // Arrange
                name = null;

                // Act -> Assert
                provider.Initialize(name, config);
            }

            [DisplayFact]
            public void Initializes()
            {
                // Arrange -> Act
                provider.Initialize(name, config);

                // Assert
                Assert.Equal(provider.ApplicationName, provider.Name);
                Assert.Equal(provider.ApplicationName, provider.Description);
            }
        }

        public class GetPropertyValuesMethod : ServiceSettingsProviderTests
        {
            private readonly SettingsContext context = new SettingsContext();
            private readonly SettingsPropertyCollection properties = new SettingsPropertyCollection();

            [DisplayFact]
            public void NoConfig_ReturnsDefaultValues()
            {
                // Arrange
                provider.GetSettingsReader = () => new StringReader("");
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

            [DisplayFact]
            public void SerializeAsXml_IsNil_DoesNotSetValue()
            {
                // Arrange
                provider.GetSettingsReader = () => new StringReader(Resources.SerializeAsXmlIsNilConfig);
                var property = SettingsUtil.CreateProperty<XmlSerializable>("myProp");
                property.SerializeAs = SettingsSerializeAs.Xml;
                properties.Add(property);

                // Act
                var values = provider.GetPropertyValues(context, properties);
                var myProp = values["myProp"].PropertyValue;

                // Assert
                Assert.Null(myProp);
            }

            [DisplayFact]
            public void SerializeAsXml_HasError_DoesNotSetValue()
            {
                // Arrange
                provider.GetSettingsReader = () => new StringReader(Resources.SerializeAsXmlErrorConfig);
                var property = SettingsUtil.CreateProperty<XmlSerializable>("myProp");
                property.SerializeAs = SettingsSerializeAs.Xml;
                properties.Add(property);

                // Act
                var values = provider.GetPropertyValues(context, properties);
                var myProp = values["myProp"].PropertyValue;

                // Assert
                Assert.Null(myProp);
            }

            [DisplayFact]
            public void HandlesSerializeAsXml()
            {
                // Arrange
                provider.GetSettingsReader = () => new StringReader(Resources.SerializeAsXmlConfig);
                var property = SettingsUtil.CreateProperty<XmlSerializable>("myProp");
                property.SerializeAs = SettingsSerializeAs.Xml;
                properties.Add(property);

                // Act
                var values = provider.GetPropertyValues(context, properties);
                var myProp = values["myProp"].PropertyValue;

                // Assert
                Assert.IsAssignableFrom<XmlSerializable>(myProp);
            }

            [DisplayFact]
            public void ReturnsValuesFromConfig()
            {
                // Arrange
                provider.GetSettingsReader = () => new StringReader(Resources.BasicConfig);
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

        public class SetPropertyValuesMethod : ServiceSettingsProviderTests
        {
            public SetPropertyValuesMethod()
            {
                provider.GetSettingsWriter = () => sw;
            }

            private readonly SettingsContext context = new SettingsContext();
            private readonly SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
            private readonly StringWriter sw = new StringWriter();

            [DisplayFact]
            public void SetsValuesInConfig()
            {
                // Arrange
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

            [DisplayFact]
            public void HandlesSerializeAsXml()
            {
                // Arrange
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

            [DisplayFact]
            public void SerializesTimeSpanInHumanReadableFormat()
            {
                // Arrange
                var value = SettingsUtil.CreatePropertyValue<TimeSpan>("myProp");
                value.PropertyValue = TimeSpan.Zero;
                values.Add(value);

                // Act
                provider.SetPropertyValues(context, values);

                // Assert
                Assert.Equal(Resources.TimeSpanConfig, sw.ToString(), ignoreLineEndingDifferences: true);
            }
        }

        public class IntegrationTests : IntegrationTestsBase<ServiceSettingsProviderSettings>
        {
            public IntegrationTests() : base(ServiceSettingsProviderSettings.Default) { }

            [DisplayFact]
            public void ReturnsDefaultValueIfValueIsNotPresent()
            {
                // Arrange -> Act
                var appId = settings.AppId;

                // Assert
                Assert.Equal(247080U, appId);
            }

            [DisplayFact]
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

            [DisplayFact]
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

            [DisplayFact]
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

            [DisplayFact]
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

            [DisplayFact]
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
