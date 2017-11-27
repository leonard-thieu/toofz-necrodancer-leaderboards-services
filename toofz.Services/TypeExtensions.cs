using System;

namespace toofz
{
    /// <summary>
    /// Contains extension methods for <see cref="Type"/>.
    /// </summary>
    internal static class TypeExtensions
    {
        // Used for generic types.
        // TODO: What's the difference between this and the normal way?
        public static string GetSimpleFullName(this Type type) => type.Namespace + "." + type.Name;
    }
}
