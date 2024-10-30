using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Magnet
{
    /// <summary>
    /// type tool
    /// </summary>
    public static class TypeUtils
    {
        private static MethodInfo AssignBinaryExpressionMake;

        static TypeUtils()
        {
            // 获取 AssignBinaryExpression.Make()
            var binaryExprType = typeof(Expression).Assembly.GetType("System.Linq.Expressions.AssignBinaryExpression");
            AssignBinaryExpressionMake = binaryExprType.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => (m.Attributes & MethodAttributes.HideBySig) != 0 && m.Name == "Make").FirstOrDefault();
        }
        #region Field Getter Setter

        /// <summary>
        /// 创建不安全的字段赋值委托 <br/>
        /// 使用 AssignBinaryExpressionMake 创建赋值表达式，以跳过CanWrite检查 对readonly赋值<br/>
        /// 速度比FieldInfo.SetValue()快很多
        /// </summary>
        /// <param name="field"></param>
        /// <returns>当字段为静态时，参数TInstance为null</returns>
        public static Action<TInstance, TValue> CreateFieldSetter<TInstance, TValue>(FieldInfo field)
        {
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var valueParam = Expression.Parameter(typeof(Object), "value");
            UnaryExpression instanceCast = null;
            if (!field.IsStatic)
            {
                instanceCast = Expression.Convert(instanceParam, field.DeclaringType);
            }
            var valueCast = Expression.Convert(valueParam, field.FieldType);
            var fieldAccess = Expression.Field(instanceCast, field);
            var assign = AssignBinaryExpressionMake.Invoke(null, new Object[] { fieldAccess, valueCast, false }) as Expression;
            var lambda = Expression.Lambda<Action<TInstance, TValue>>(assign, instanceParam, valueParam);
            return lambda.Compile();
        }


        /// <summary>
        /// 创建不安全的字段赋值委托 <br/>
        /// 使用 AssignBinaryExpressionMake 创建赋值表达式，以跳过CanWrite检查 对readonly赋值<br/>
        /// 速度比FieldInfo.SetValue()快很多
        /// </summary>
        /// <param name="field"></param>
        /// <returns>当字段为静态时，参数TInstance为null</returns>
        public static Func<TInstance, TResult> CreateFieldGetter<TInstance, TResult>(FieldInfo field)
        {
            var instanceType = typeof(TInstance);
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            Expression instanceCast = null;
            if (!field.IsStatic)
            {
                instanceCast = Expression.Convert(instanceParam, field.DeclaringType);
            }
            var fieldAccess = Expression.Field(instanceCast, field);
            var lambda = Expression.Lambda<Func<TInstance, TResult>>(fieldAccess, instanceParam);
            return lambda.Compile();
        }


        #endregion

        #region CreateMethodPointer
        /// <summary>
        /// 创建一个不安全的带有返回值的方法指针
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<TResult> CreateMethodPointer<TResult>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<TResult>)pointer;
        }

        /// <summary>
        /// 创建一个不安全的带有返回值的方法指针
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <typeparam name="TParam1">参数1类型</typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<TParam1, TResult> CreateMethodPointer<TResult, TParam1>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<TParam1, TResult>)pointer;
        }

        /// <summary>
        /// 创建一个不安全的带有返回值的方法指针
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <typeparam name="TParam1">参数1类型</typeparam>
        /// <typeparam name="TParam2">参数2类型</typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<in TParam1, in TParam2, TResult> CreateMethodPointer<TResult, TParam1, TParam2>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<in TParam1, in TParam2, TResult>)pointer;
        }

        /// <summary>
        /// 创建一个不安全的带有返回值的方法指针
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <typeparam name="TParam1">参数1类型</typeparam>
        /// <typeparam name="TParam2">参数2类型</typeparam>
        /// <typeparam name="TParam3">参数3类型</typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<in TParam1, in TParam2, in TParam3, TResult> CreateMethodPointer<TResult, TParam1, TParam2, TParam3>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<in TParam1, in TParam2, in TParam3, TResult>)pointer;
        }

        /// <summary>
        /// 创建一个不安全的带有返回值的方法指针
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <typeparam name="TParam1">参数1类型</typeparam>
        /// <typeparam name="TParam2">参数2类型</typeparam>
        /// <typeparam name="TParam3">参数3类型</typeparam>
        /// <typeparam name="TParam4">参数4类型</typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<in TParam1, in TParam2, in TParam3, in TParam4, TResult> CreateMethodPointer<TResult, TParam1, TParam2, TParam3, TParam4>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<in TParam1, in TParam2, in TParam3, in TParam4, TResult>)pointer;
        }

        #endregion

        #region CreateActionPointer

        /// <summary>
        /// 创建一个不安全的无返回值的方法指针
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<void> CreateActionPointer(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<void>)pointer;
        }

        /// <summary>
        /// 创建一个不安全的无返回值的方法指针
        /// </summary>
        /// <typeparam name="TParam1">参数1类型</typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<TParam1, void> CreateActionPointer<TParam1>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<TParam1, void>)pointer;
        }

        /// <summary>
        /// 创建一个不安全的无返回值的方法指针
        /// </summary>
        /// <typeparam name="TParam1">参数1类型</typeparam>
        /// <typeparam name="TParam2">参数2类型</typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<in TParam1, in TParam2, void> CreateActionPointer<TParam1, TParam2>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<in TParam1, in TParam2, void>)pointer;
        }

        /// <summary>
        /// 创建一个不安全的无返回值的方法指针
        /// </summary>
        /// <typeparam name="TParam1">参数1类型</typeparam>
        /// <typeparam name="TParam2">参数2类型</typeparam>
        /// <typeparam name="TParam3">参数3类型</typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<in TParam1, in TParam2, in TParam3, void> CreateActionPointer<TParam1, TParam2, TParam3>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<in TParam1, in TParam2, in TParam3, void>)pointer;
        }

        /// <summary>
        /// 创建一个不安全的无返回值的方法指针
        /// </summary>
        /// <typeparam name="TParam1">参数1类型</typeparam>
        /// <typeparam name="TParam2">参数2类型</typeparam>
        /// <typeparam name="TParam3">参数3类型</typeparam>
        /// <typeparam name="TParam4">参数4类型</typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<in TParam1, in TParam2, in TParam3, in TParam4, void> CreateActionPointer<TParam1, TParam2, TParam3, TParam4>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<in TParam1, in TParam2, in TParam3, in TParam4, void>)pointer;
        }
        #endregion

        #region CleanTypeName
        /// <summary>
        /// Clears the type full name of the generic parameter 
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String CleanTypeName(String typeFullName)
        {
            return typeFullName.Split(['`', '<'], StringSplitOptions.RemoveEmptyEntries)[0];
        }


        /// <summary>
        /// Clears the type full name of the generic parameter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String CleanTypeName(this Type type)
        {
            return type.FullName.Split(['`', '<'], StringSplitOptions.RemoveEmptyEntries)[0];
        }

        /// <summary>
        /// Clears the type full name of the generic parameter
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String CleanTypeName(this ITypeSymbol typeFullName)
        {
            return typeFullName.ToDisplayString().Split(['`', '<'], StringSplitOptions.RemoveEmptyEntries)[0];
        }
        #endregion

    }
}
