using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace toofz.NecroDancer.Leaderboards.Services
{
    [TypeConverter(typeof(EncryptedSecretConverter))]
    public sealed class EncryptedSecret
    {
        /// <summary>
        /// Initializes an instance of the <see cref="EncryptedSecret"/> class.
        /// </summary>
        /// <param name="secret">The secret to encrypt.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="secret"/> cannot be null or empty.
        /// </exception>
        public EncryptedSecret(string secret)
        {
            var (encrypted, salt) = Secrets.Encrypt(secret);
            Initialize(encrypted, salt);
        }

        internal EncryptedSecret(byte[] encrypted, byte[] salt)
        {
            Initialize(encrypted, salt);
        }

        void Initialize(byte[] encrypted, byte[] salt)
        {
            Secret = Array.AsReadOnly(encrypted);
            Salt = Array.AsReadOnly(salt);
        }

        internal ReadOnlyCollection<byte> Secret { get; private set; }

        ReadOnlyCollection<byte> salt;
        internal ReadOnlyCollection<byte> Salt
        {
            get => salt;
            private set
            {
                if (value.Count < 8 || value.Count > byte.MaxValue)
                {
                    throw new InvalidOperationException($"The size of the salt must be at least 8 bytes and no more than {byte.MaxValue} bytes.");
                }
                SaltSize = Convert.ToByte(value.Count);
                salt = value;
            }
        }

        internal byte SaltSize { get; private set; }

        /// <summary>
        /// Decrypts the encrypted secret.
        /// </summary>
        /// <returns>
        /// The decrypted secret.
        /// </returns>
        public string Decrypt()
        {
            return Secrets.Decrypt(Secret.ToArray(), Salt.ToArray());
        }
    }
}
