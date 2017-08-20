using System.Linq;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace toofz.NecroDancer.Leaderboards.Services.Tests
{
    class SecretsTests
    {
        [TestClass]
        public class Encrypt
        {
            [TestMethod]
            public void SecretIsNull_ThrowsArgumentException()
            {
                // Arrange
                string secret = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    Secrets.Encrypt(secret);
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
                    Secrets.Encrypt(secret);
                });
            }

            [TestMethod]
            public void ReturnsEncryptedSecret()
            {
                // Arrange
                string secret = "mySecret";

                // Act
                var (encrypted, salt) = Secrets.Encrypt(secret);

                // Assert
                Assert.IsTrue(encrypted.Any());
            }

            [TestMethod]
            public void ReturnsSalt()
            {
                // Arrange
                string secret = "mySecret";

                // Act
                var (encrypted, salt) = Secrets.Encrypt(secret);

                // Assert
                Assert.IsTrue(salt.Any());
            }
        }

        [TestClass]
        public class Decrypt
        {
            [TestMethod]
            public void EncryptedIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                byte[] encrypted = null;
                byte[] salt = new byte[8];

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    Secrets.Decrypt(encrypted, salt);
                });
            }

            [TestMethod]
            public void SaltIsNull_ThrowsArgumentNullException()
            {
                // Arrange
                byte[] encrypted = new byte[8];
                byte[] salt = null;

                // Act -> Assert
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    Secrets.Decrypt(encrypted, salt);
                });
            }

            [TestMethod]
            public void SaltIsLessThan8Bytes_ThrowsArgumentException()
            {
                // Arrange
                byte[] encrypted = new byte[8];
                byte[] salt = new byte[0];

                // Act -> Assert
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    Secrets.Decrypt(encrypted, salt);
                });
            }

            [TestMethod]
            public void ReturnsDecryptedSecret()
            {
                // Arrange
                var secret = "mySecret";
                var (encrypted, salt) = Secrets.Encrypt(secret);

                // Act
                var decrypted = Secrets.Decrypt(encrypted, salt);

                // Assert
                Assert.AreEqual(secret, decrypted);
            }
        }
    }
}
