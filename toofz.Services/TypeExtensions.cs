﻿using System;

namespace toofz
{
    internal static class TypeExtensions
    {
        public static string GetSimpleFullName(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.Namespace + "." + type.Name;
        }
    }
}
