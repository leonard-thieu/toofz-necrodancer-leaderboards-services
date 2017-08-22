using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace toofz.NecroDancer.Leaderboards.Services
{
    /// <summary>
    /// Converts <see cref="EncryptedSecret"/> to and from <see cref="string"/>.
    /// </summary>
    sealed class EncryptedSecretConverter : TypeConverter
    {
        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="string"/>.
        /// </summary>
        /// <param name="context">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <param name="value">An instance of <see cref="EncryptedSecret"/>.</param>
        /// <param name="destinationType">The <see cref="string"/> type.</param>
        /// <returns>
        /// A base64 encoded string representation of <paramref name="value"/>.
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        /// <paramref name="value"/> is not an instance of <see cref="EncryptedSecret"/>.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// <paramref name="destinationType"/> is not the <see cref="string"/> type.
        /// </exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!((value is EncryptedSecret) &&
                  (destinationType == typeof(string))))
            {
                throw GetConvertToException(value, destinationType);
            }

            var encryptedSecret = (EncryptedSecret)value;
            var concatenated = new List<byte>();
            concatenated.Add((byte)encryptedSecret.Salt.Count);
            concatenated.AddRange(encryptedSecret.Salt);
            concatenated.AddRange(encryptedSecret.Secret);

            return Convert.ToBase64String(concatenated.ToArray());
        }

        /// <summary>
        /// Gets a value indicating whether this converter can convert an object in the given source type to an instance of <see cref="EncryptedSecret"/>.
        /// </summary>
        /// <param name="context">Not used.</param>
        /// <param name="sourceType">The source type to convert from.</param>
        /// <returns>
        /// True, if this converter can convert an object in the given source type to an instance of <see cref="EncryptedSecret"/>; otherwise, false.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Converts the given base64 encoded string to an instance of <see cref="EncryptedSecret"/>.
        /// </summary>
        /// <param name="context">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <param name="value">The base64 encoded string to convert to an instance of <see cref="EncryptedSecret"/>.</param>
        /// <returns>
        /// An instance of <see cref="EncryptedSecret"/> converted from the base64 encoded string.
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        /// <paramref name="value"/> is not an instance of <see cref="string"/>.
        /// </exception>
        /// <exception cref="System.FormatException">
        /// The length of <paramref name="value"/>, ignoring white-space characters, is not zero or a multiple of 4.
        /// </exception>
        /// <exception cref="System.FormatException">
        /// The format of <paramref name="value"/> is invalid. <paramref name="value"/> contains a non-base-64 character, 
        /// more than two padding characters, or a non-white space-character among the padding characters.
        /// </exception>
        /// <exception cref="System.FormatException">
        /// The size of the salt does not match the expected size.
        /// </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                throw GetConvertFromException(value);
            }

            var concatenated = Convert.FromBase64String((string)value);
            var saltSize = concatenated[0];
            var salt = concatenated.Skip(1).Take(saltSize).ToArray();
            if (salt.Length != saltSize)
            {
                throw new FormatException($"Expected a salt of {saltSize} bytes but got {salt.Length} bytes.");
            }
            var encrypted = concatenated.Skip(saltSize + 1).ToArray();

            return new EncryptedSecret(encrypted, salt);
        }
    }
}