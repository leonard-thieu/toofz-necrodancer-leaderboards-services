using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using toofz.Services.Tests.Properties;
using toofz.TestsShared;

namespace toofz.Services.Tests
{
    public class ServiceSettingsProviderTests
    {
        [TestClass]
        public class ApplicationName
        {
            [TestMethod]
            public void SetToNull_ThrowsArgumentNullException()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                string applicationName = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    provider.ApplicationName = applicationName;
                });
            }

            [TestMethod]
            public void ReturnsADefaultValue()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();

                // Act
                var applicationName = provider.ApplicationName;

                // Assert
                Assert.AreEqual("toofz", applicationName);
            }

            [TestMethod]
            public void GetSetBehavior()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                string applicationName = "My Application";

                // Act
                provider.ApplicationName = applicationName;

                // Assert
                Assert.AreEqual(applicationName, provider.ApplicationName);
            }
        }

        [TestClass]
        public class GetSettingsReader
        {
            [TestMethod]
            public void SetToNull_ThrowsArgumentNullException()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                Func<TextReader> getSettingsReader = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    provider.GetSettingsReader = getSettingsReader;
                });
            }

            [TestMethod]
            public void ReturnsADefaultValue()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();

                // Act
                var getSettingsReader = provider.GetSettingsReader;

                // Assert
                Assert.IsNotNull(getSettingsReader);
            }

            [TestMethod]
            public void GetSetBehavior()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                Func<TextReader> getSettingsReader = () => new StringReader("");

                // Act
                provider.GetSettingsReader = getSettingsReader;

                // Assert
                Assert.AreEqual(getSettingsReader, provider.GetSettingsReader);
            }
        }

        [TestClass]
        public class GetSettingsWriter
        {
            [TestMethod]
            public void SetToNull_ThrowsArgumentNullException()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                Func<TextWriter> getSettingsWriter = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    provider.GetSettingsWriter = getSettingsWriter;
                });
            }

            [TestMethod]
            public void ReturnsADefaultValue()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();

                // Act
                var getSettingsWriter = provider.GetSettingsWriter;

                // Assert
                Assert.IsNotNull(getSettingsWriter);
            }

            [TestMethod]
            public void GetSetBehavior()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                Func<TextWriter> getSettingsWriter = () => new StringWriter();

                // Act
                provider.GetSettingsWriter = getSettingsWriter;

                // Assert
                Assert.AreEqual(getSettingsWriter, provider.GetSettingsWriter);
            }
        }

        [TestClass]
        public class Initialize
        {
            [TestMethod]
            public void NameIsNull_DoesNotThrowArgumentNullException()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();

                // Act -> Assert
                provider.Initialize(null, new NameValueCollection());
            }

            [TestMethod]
            public void Initializes()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();

                // Act
                provider.Initialize("myName", new NameValueCollection());

                // Assert
                Assert.AreEqual(provider.ApplicationName, provider.Name);
                Assert.AreEqual(provider.ApplicationName, provider.Description);
            }
        }

        [TestClass]
        public class GetPropertyValues
        {
            [TestMethod]
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
                Assert.AreEqual(2, values.Count);
                Assert.AreEqual("myDefaultValue1", values["myProp1"].PropertyValue);
                Assert.AreEqual("myDefaultValue2", values["myProp2"].PropertyValue);
            }

            [TestMethod]
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
                Assert.IsInstanceOfType(myProp, typeof(XmlSerializable));
            }

            [TestMethod]
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
                Assert.AreEqual(2, values.Count);
                Assert.AreEqual("mySerializedValue1", values["myProp1"].PropertyValue);
                Assert.AreEqual("mySerializedValue2", values["myProp2"].PropertyValue);
            }
        }

        [TestClass]
        public class SetPropertyValues
        {
            [TestMethod]
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
                Assert.AreEqual(Resources.BasicConfig, sw.ToString());
            }

            [TestMethod]
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
                Assert.That.NormalizedAreEqual(Resources.SerializeAsXmlConfig, sw.ToString());
            }

            [TestMethod]
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
                Assert.That.NormalizedAreEqual(Resources.TimeSpanConfig, sw.ToString());
            }
        }

        [TestClass]
        [TestCategory("Integration")]
        public class IntegrationTests
        {
            public IntegrationTests()
            {
                File.Delete(ServiceSettingsProvider.ConfigFileName);
                settings = ServiceSettingsProviderSettings.Default;
                settings.Reload();
            }

            private ServiceSettingsProviderSettings settings;

            [TestMethod]
            public void ReturnsDefaultValueIfValueIsNotPresent()
            {
                // Arrange -> Act
                var appId = settings.AppId;

                // Assert
                Assert.AreEqual(247080U, appId);
            }

            [TestMethod]
            public void SavesChangedSetting()
            {
                // Arrange
                settings.ForceSave = true;

                // Act
                settings.Save();
                settings.Reload();
                var appId = settings.AppId;

                // Assert
                Assert.IsTrue(settings.ForceSave);
            }

            [TestMethod]
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
                Assert.AreEqual(247080.ToString(), appIdEl.Value);
            }

            [TestMethod]
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
                Assert.AreEqual("00:02:00", durationEl.Value);
            }

            [TestMethod]
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
                Assert.AreEqual("00:03:54", durationEl.Value);
            }

            [TestMethod]
            public void ReadsTimeSpanInHumanReadableFormat()
            {
                // Arrange
                settings.Duration = TimeSpan.FromSeconds(234);
                settings.Save();

                // Act
                settings.Reload();
                var duration = settings.Duration;

                // Assert
                Assert.AreEqual(TimeSpan.FromSeconds(234), duration);
            }
        }
    }
}
