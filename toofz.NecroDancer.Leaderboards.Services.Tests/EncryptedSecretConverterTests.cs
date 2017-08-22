using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace toofz.NecroDancer.Leaderboards.Services.Tests
{
    class EncryptedSecretConverterTests
    {
        [TestClass]
        public class ConvertTo
        {
            [TestMethod]
            public void ValueIsNotEncryptedSecret_ThrowsNotSupportedException()
            {
                // Arrange
                var converter = new EncryptedSecretConverter();
                object value = new object();
                Type destinationType = typeof(string);

                // Act -> Assert
                Assert.ThrowsException<NotSupportedException>(() =>
                {
                    converter.ConvertTo(null, null, value, destinationType);
                });
            }


            [TestMethod]
            public void DestinationTypeIsNotStringType_ThrowsNotSupportedException()
            {
                // Arrange
                var converter = new EncryptedSecretConverter();
                object value = new EncryptedSecret("mySecret");
                Type destinationType = typeof(object);

                // Act -> Assert
                Assert.ThrowsException<NotSupportedException>(() =>
                {
                    converter.ConvertTo(null, null, value, destinationType);
                });
            }

            [TestMethod]
            public void ReturnsEncryptedSecretAsBase64EncodedString()
            {
                // Arrange
                var converter = new EncryptedSecretConverter();
                object value = new EncryptedSecret("mySecret");
                Type destinationType = typeof(string);

                // Act
                var encoded = converter.ConvertTo(null, null, value, destinationType);

                // Assert
                Assert.IsInstanceOfType(encoded, typeof(string));
            }
        }

        [TestClass]
        public class CanConvertFrom
        {
            [TestMethod]
            public void SourceTypeIsStringType_ReturnsTrue()
            {
                // Arrange
                var converter = new EncryptedSecretConverter();
                var sourceType = typeof(string);

                // Act
                var canConvertFrom = converter.CanConvertFrom(null, sourceType);

                // Assert
                Assert.IsTrue(canConvertFrom);
            }

            [TestMethod]
            public void ReturnsFalse()
            {
                // Arrange
                var converter = new EncryptedSecretConverter();
                var sourceType = typeof(object);

                // Act
                var canConvertFrom = converter.CanConvertFrom(null, sourceType);

                // Assert
                Assert.IsFalse(canConvertFrom);
            }
        }

        [TestClass]
        public class ConvertFrom
        {
            [TestMethod]
            public void ValueIsNotAString_ThrowsNotSupportedException()
            {
                // Arrange
                var converter = new EncryptedSecretConverter();
                object value = new object();

                // Act -> Assert
                Assert.ThrowsException<NotSupportedException>(() =>
                {
                    converter.ConvertFrom(null, null, value);
                });
            }

            [TestMethod]
            public void ReturnsEncryptedSecret()
            {
                // Arrange
                var converter = new EncryptedSecretConverter();
                object value = converter.ConvertTo(new EncryptedSecret("mySecret"), typeof(string));

                // Act
                var encryptedSecret = converter.ConvertFrom(value);

                // Assert
                Assert.IsInstanceOfType(encryptedSecret, typeof(EncryptedSecret));
            }
        }
    }
}
