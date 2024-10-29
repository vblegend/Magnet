using Magnet.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Magnet
{
    public static class TypeUtils
    {
        private static MethodInfo AssignBinaryExpressionMake;

        static TypeUtils()
        {
            // 获取 AssignBinaryExpression.Make()
            var binaryExprType = typeof(Expression).Assembly.GetType("System.Linq.Expressions.AssignBinaryExpression");
            AssignBinaryExpressionMake = binaryExprType.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => (m.Attributes & MethodAttributes.HideBySig) != 0 && m.Name == "Make").FirstOrDefault();
        }

        /// <summary>
        /// 创建不安全的字段赋值委托 <br/>
        /// 速度比FieldInfo.SetValue()快很多
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Action<AbstractScript, Object> CreateFieldSetter(FieldInfo field)
        {
            var instanceParam = Expression.Parameter(typeof(AbstractScript), "instance");
            var valueParam = Expression.Parameter(typeof(Object), "value");
            var instanceCast = Expression.Convert(instanceParam, field.DeclaringType);
            var valueCast = Expression.Convert(valueParam, field.FieldType);
            var fieldAccess = Expression.Field(instanceCast, field);
            // 使用 AssignBinaryExpressionMake 创建赋值表达式，以跳过CanWrite检查 对readonly赋值
            var assign = AssignBinaryExpressionMake.Invoke(null, new Object[] { fieldAccess, valueCast, false }) as Expression;
            var lambda = Expression.Lambda<Action<AbstractScript, Object>>(assign, instanceParam, valueParam);
            return lambda.Compile();
        }









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
