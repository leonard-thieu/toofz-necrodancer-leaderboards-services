using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace toofz.NecroDancer.Leaderboards.Services.Tests
{
    class EncryptedSecretTests
    {
        [TestClass]
        public class Constructor
        {
            [TestMethod]
            public void SecretIsNull_ThrowsArgumentException()
            {
                // Arrange
                string secret = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new EncryptedSecret(secret);
                });
            }

            [TestMethod]
            public void SecretIsEmpty_ThrowsArgumentException()
            {
                // Arrange
                string secret = "";

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    new EncryptedSecret(secret);
                });
            }

            [TestMethod]
            public void ReturnsEncryptedSecret()
            {
                // Arrange
                string secret = "mySecret";

                // Act
                var encryptedSecret = new EncryptedSecret(secret);

                // Assert
                Assert.IsInstanceOfType(encryptedSecret, typeof(EncryptedSecret));
            }
        }

        [TestClass]
        public class Decrypt
        {
            [TestMethod]
            public void ReturnsDecryptedSecret()
            {
                // Arrange
                string secret = "mySecret";
                var encryptedSecret = new EncryptedSecret(secret);

                // Act
                var decryptedSecret = encryptedSecret.Decrypt();

                // Assert
                Assert.AreEqual(secret, decryptedSecret);
            }
        }
    }
}
