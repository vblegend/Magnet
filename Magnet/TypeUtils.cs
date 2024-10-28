using Microsoft.CodeAnalysis;
using System;
 
namespace Magnet
{
    public static class TypeUtils
    {


        /// <summary>
        /// Clears the type full name of the generic parameter 
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        public static String CleanTypeName(String typeFullName)
        {
            return typeFullName.Split(['`', '<'], StringSplitOptions.RemoveEmptyEntries)[0];
        }


        /// <summary>
        /// Clears the type full name of the generic parameter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static String CleanTypeName(this Type type)
        {
            return type.FullName.Split(['`', '<'], StringSplitOptions.RemoveEmptyEntries)[0];
        }

        /// <summary>
        /// Clears the type full name of the generic parameter
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        public static String CleanTypeName(this ITypeSymbol typeFullName)
        {
            return typeFullName.ToDisplayString().Split(['`', '<'], StringSplitOptions.RemoveEmptyEntries)[0];
        }

    }
}
