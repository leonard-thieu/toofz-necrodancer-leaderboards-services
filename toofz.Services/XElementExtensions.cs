using System;
using System.Xml.Linq;

namespace toofz.Services
{
    /// <summary>
    /// Contains extension methods for <see cref="XElement"/>.
    /// </summary>
    internal static class XElementExtensions
    {
        internal static readonly XName Nil = XName.Get("nil", "http://www.w3.org/2001/XMLSchema-instance");

        /// <summary>
        /// Checks if an <see cref="XElement"/> is nil.
        /// </summary>
        /// <param name="el">The <see cref="XElement"/> to check.</param>
        /// <returns>
        /// true, if <paramref name="el"/> is nil; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="el"/> is null.
        /// </exception>
        public static bool IsNil(this XElement el)
        {
            if (el == null)
                throw new ArgumentNullException(nameof(el));

            return (bool?)el.Attribute(Nil) ?? false;
        }
    }
}
