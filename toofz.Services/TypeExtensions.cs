using System;

namespace toofz
{
    internal static class TypeExtensions
    {
        public static string GetSimpleFullName(this Type type) => type.Namespace + "." + type.Name;
    }
}
