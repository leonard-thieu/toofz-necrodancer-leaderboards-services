using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace toofz.Services
{
    [XmlRoot(EncryptedSecretName)]
    public sealed class EncryptedSecret : IXmlSerializable
    {
        private const int SaltSize = 8;

        // Using the host's physical address is a compromise between security and ease of use.
        // One of the goals of the toofz projects is for the code to be easy to evaluate and understand.
        // Making it straight-forward to run projects is a step in this direction, however, it does mean that 
        // we can't ask users to secure a password.
        private static readonly string Password = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                                                   where nic.OperationalStatus == OperationalStatus.Up
                                                   select nic.GetPhysicalAddress())
                                                   .FirstOrDefault()
                                                   .ToString();

        private const string EncryptedSecretName = "encryptedSecret";
        private const string SecretName = "secret";
        private const string SaltName = "salt";
        private const string IterationsName = "iterations";

        // Required for XML serialization
        private EncryptedSecret() { }

        /// <summary>
        /// Initializes an instance of the <see cref="EncryptedSecret"/> class.
        /// </summary>
        /// <param name="secret">The secret to encrypt.</param>
        /// <param name="iterations">The number of iterations used to derive the key.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="secret"/> cannot be null or empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="iterations"/> must be a positive number.
        /// </exception>
        public EncryptedSecret(string secret, int iterations) : this()
        {
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentException($"{nameof(secret)} cannot be null or empty.", nameof(secret));
            // The constructor for Rfc2898DeriveBytes performs the same validation but returns a generic, non-descriptive exception message.
            if (iterations < 1)
                throw new ArgumentOutOfRangeException(nameof(iterations), iterations, $"{nameof(iterations)} must be a positive number.");

            using (var pbkdf2 = new Rfc2898DeriveBytes(Password, SaltSize, iterations))
            using (var alg = Rijndael.Create())
            {
                alg.Key = pbkdf2.GetBytes(32);
                alg.IV = pbkdf2.GetBytes(16);

                using (var inStream = new MemoryStream(Encoding.UTF8.GetBytes(secret)))
                using (var outStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(outStream, alg.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    inStream.CopyTo(cryptoStream);
                    cryptoStream.FlushFinalBlock();

                    encrypted = outStream.ToArray();
                    salt = pbkdf2.Salt;
                }
            }
            this.iterations = iterations;
        }

        private byte[] encrypted;
        private byte[] salt;
        private int iterations;

        /// <summary>
        /// Decrypts the encrypted secret.
        /// </summary>
        /// <returns>
        /// The decrypted secret.
        /// </returns>
        public string Decrypt()
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(Password, salt, iterations))
            using (var alg = Rijndael.Create())
            {
                alg.Key = pbkdf2.GetBytes(32);
                alg.IV = pbkdf2.GetBytes(16);

                using (var inStream = new MemoryStream(encrypted))
                using (var outStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(outStream, alg.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    inStream.CopyTo(cryptoStream);
                    cryptoStream.FlushFinalBlock();
                    outStream.Position = 0;

                    using (var sr = new StreamReader(outStream))
                    {
                        var decrypted = sr.ReadToEnd();

                        return decrypted;
                    }
                }
            }
        }

        #region IXmlSerializable Members

        XmlSchema IXmlSerializable.GetSchema() => null;

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="XmlReader"/> stream from which the object is deserialized.
        /// </param>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.ReadStartElement(EncryptedSecretName);
            encrypted = Convert.FromBase64String(reader.ReadElementContentAsString(SecretName, ""));
            salt = Convert.FromBase64String(reader.ReadElementContentAsString(SaltName, ""));
            iterations = reader.ReadElementContentAsInt(IterationsName, "");
            reader.ReadEndElement();
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="XmlWriter"/> stream to which the object is serialized.
        /// </param>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(SecretName);
            writer.WriteBase64(encrypted, 0, encrypted.Length);
            writer.WriteEndElement();

            writer.WriteStartElement(SaltName);
            writer.WriteBase64(salt, 0, salt.Length);
            writer.WriteEndElement();

            writer.WriteStartElement(IterationsName);
            writer.WriteValue(iterations);
            writer.WriteEndElement();
        }

        #endregion
    }
}
