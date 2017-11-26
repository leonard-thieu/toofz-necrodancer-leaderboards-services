using System;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace toofz.Services.Tests
{
    public class EncryptedSecretTests
    {
        public class Constructor
        {
            [Fact]
            public void SecretIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                string secret = null;
                int iterations = 1000;

                // Act -> Assert
                Assert.Throws<ArgumentNullException>(() =>
                {
                    new EncryptedSecret(secret, iterations);
                });
            }

            [Fact]
            public void SecretIsEmpty_ThrowsArgumentException()
            {
                // Arrange
                string secret = "";
                int iterations = 1000;

                // Act -> Assert
                Assert.Throws<ArgumentException>(() =>
                {
                    new EncryptedSecret(secret, iterations);
                });
            }

            [Fact]
            public void ReturnsEncryptedSecret()
            {
                // Arrange
                string secret = "mySecret";
                int iterations = 1000;

                // Act
                var encryptedSecret = new EncryptedSecret(secret, iterations);

                // Assert
                Assert.IsAssignableFrom<EncryptedSecret>(encryptedSecret);
            }
        }

        public class DecryptMethod
        {
            [Fact]
            public void ReturnsDecryptedSecret()
            {
                // Arrange
                string secret = "mySecret";
                int iterations = 1000;
                var encryptedSecret = new EncryptedSecret(secret, iterations);

                // Act
                var decryptedSecret = encryptedSecret.Decrypt();

                // Assert
                Assert.Equal(secret, decryptedSecret);
            }
        }

        public class GetSchemaMethod
        {
            [Fact]
            public void ReturnsNull()
            {
                // Arrange
                var encryptedSecret = new EncryptedSecret("mySecret", 1000);
                var xml = (IXmlSerializable)encryptedSecret;

                // Act
                var schema = xml.GetSchema();

                // Assert
                Assert.Null(schema);
            }
        }

        public class SerializationTests
        {
            [Fact]
            public void SerializesAndDeserializes()
            {
                // Arrange
                var provider = new ServiceSettingsProvider();
                var sw = new StringWriter();
                provider.GetSettingsWriter = () => sw;
                var context = new SettingsContext();
                var values = new SettingsPropertyValueCollection();
                var property = SettingsUtil.CreateProperty<EncryptedSecret>("myProp");
                property.SerializeAs = SettingsSerializeAs.Xml;
                var value = new SettingsPropertyValue(property);
                var encryptedSecret = new EncryptedSecret("mySecret", 1000);
                value.PropertyValue = encryptedSecret;
                values.Add(value);

                // Act
                provider.SetPropertyValues(context, values);

                // Arrange
                provider.GetSettingsReader = () => new StringReader(sw.ToString());
                var properties = new SettingsPropertyCollection();
                properties.Add(property);

                // Act
                var values2 = provider.GetPropertyValues(context, properties);
                var myProp = values2["myProp"].PropertyValue;

                // Assert
                Assert.IsAssignableFrom<EncryptedSecret>(myProp);
                var encryptedSecret2 = (EncryptedSecret)myProp;
                Assert.Equal("mySecret", encryptedSecret2.Decrypt());
            }
        }
    }
}
