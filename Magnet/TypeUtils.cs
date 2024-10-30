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
        private delegate Expression MakeDelegate(Expression left, Expression right, Boolean v);
        /// <summary>
        /// System.Linq.Expressions.AssignBinaryExpression.Make()
        /// </summary>
        private static MakeDelegate MakeAssignExpression;


        static TypeUtils()
        {
            // 获取 AssignBinaryExpression.Make()
            var binaryExprType = typeof(Expression).Assembly.GetType("System.Linq.Expressions.AssignBinaryExpression");
            var makeMethod = binaryExprType.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => (m.Attributes & MethodAttributes.HideBySig) != 0 && m.Name == "Make").FirstOrDefault();
            MakeAssignExpression = (MakeDelegate)Delegate.CreateDelegate(typeof(MakeDelegate), null, makeMethod);
            if (MakeAssignExpression == null)
            {
                throw new Exception("Unable to locate to System. Linq. Expressions. Make AssignBinaryExpression method.");
            }
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
            var assign = MakeAssignExpression(fieldAccess, valueCast, false);
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

        #region CreateStaticMethodPointer(Static)
        /// <summary>
        /// 创建一个不安全的带有返回值的方法指针
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<TResult> CreateStaticMethodPointer<TResult>(MethodInfo method)
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
        public unsafe static delegate*<TParam1, TResult> CreateStaticMethodPointer<TParam1, TResult>(MethodInfo method)
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
        public unsafe static delegate*<TParam1, TParam2, TResult> CreateStaticMethodPointer<TParam1, TParam2, TResult>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<TParam1, TParam2, TResult>)pointer;
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
        public unsafe static delegate*<TParam1, TParam2, TParam3, TResult> CreateStaticMethodPointer<TParam1, TParam2, TParam3, TResult>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<TParam1, TParam2, TParam3, TResult>)pointer;
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
        public unsafe static delegate*<TParam1, TParam2, TParam3, TParam4, TResult> CreateStaticMethodPointer<TParam1, TParam2, TParam3, TParam4, TResult>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<TParam1, TParam2, TParam3, TParam4, TResult>)pointer;
        }

        #endregion

        #region CreateStaticActionPointer(Static)

        /// <summary>
        /// 创建一个不安全的无返回值的方法指针
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static delegate*<void> CreateStaticActionPointer(MethodInfo method)
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
        public unsafe static delegate*<TParam1, void> CreateStaticActionPointer<TParam1>(MethodInfo method)
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
        public unsafe static delegate*<in TParam1, in TParam2, void> CreateStaticActionPointer<TParam1, TParam2>(MethodInfo method)
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
        public unsafe static delegate*<in TParam1, in TParam2, in TParam3, void> CreateStaticActionPointer<TParam1, TParam2, TParam3>(MethodInfo method)
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
        public unsafe static delegate*<in TParam1, in TParam2, in TParam3, in TParam4, void> CreateStaticActionPointer<TParam1, TParam2, TParam3, TParam4>(MethodInfo method)
        {
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            return (delegate*<in TParam1, in TParam2, in TParam3, in TParam4, void>)pointer;
        }
        #endregion

        #region CreateMethodDelegate(Instance)

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Func<TInstance, TResult> CreateMethodDelegate<TInstance, TResult>(MethodInfo method)
        {
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var call = Expression.Call(instanceParam, method);
            var lambda = Expression.Lambda<Func<TInstance, TResult>>(call, instanceParam);
            return lambda.Compile();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TParam1"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Func<TInstance, TParam1, TResult> CreateMethodDelegate<TInstance, TParam1, TResult>(MethodInfo method)
        {
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var param1 = Expression.Parameter(typeof(TParam1), "param1");
            var call = Expression.Call(instanceParam, method, param1);
            var lambda = Expression.Lambda<Func<TInstance, TParam1, TResult>>(call, instanceParam, param1);
            return lambda.Compile();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TParam1"></typeparam>
        /// <typeparam name="TParam2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Func<TInstance, TParam1, TParam2, TResult> CreateMethodDelegate<TInstance, TParam1, TParam2, TResult>(MethodInfo method)
        {
            if (method == null) return null;
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var param1 = Expression.Parameter(typeof(TParam1), "param1");
            var param2 = Expression.Parameter(typeof(TParam2), "param2");
            var call = Expression.Call(instanceParam, method, param1, param2);
            var lambda = Expression.Lambda<Func<TInstance, TParam1, TParam2, TResult>>(call, instanceParam, param1, param2);
            return lambda.Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TParam1"></typeparam>
        /// <typeparam name="TParam2"></typeparam>
        /// <typeparam name="TParam3"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        /// 
        public static Func<TInstance, TParam1, TParam2, TParam3, TResult> CreateMethodDelegate<TInstance, TParam1, TParam2, TParam3, TResult>(MethodInfo method)
        {
            if (method == null) return null;
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var param1 = Expression.Parameter(typeof(TParam1), "param1");
            var param2 = Expression.Parameter(typeof(TParam2), "param2");
            var param3 = Expression.Parameter(typeof(TParam3), "param3");
            var call = Expression.Call(instanceParam, method, param1, param2, param3);
            var lambda = Expression.Lambda<Func<TInstance, TParam1, TParam2, TParam3, TResult>>(call, instanceParam, param1, param2, param3);
            return lambda.Compile();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TParam1"></typeparam>
        /// <typeparam name="TParam2"></typeparam>
        /// <typeparam name="TParam3"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Func<TInstance, TParam1, TParam2, TParam3, TParam4, TResult>? CreateMethodDelegate<TInstance, TParam1, TParam2, TParam3, TParam4, TResult>(MethodInfo method)
        {
            if (method == null) return null;
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var param1 = Expression.Parameter(typeof(TParam1), "param1");
            var param2 = Expression.Parameter(typeof(TParam2), "param2");
            var param3 = Expression.Parameter(typeof(TParam3), "param3");
            var param4 = Expression.Parameter(typeof(TParam4), "param4");
            // 创建方法调用表达式
            var call = Expression.Call(instanceParam, method, param1, param2, param3, param4);
            // 构建表达式树并编译成委托
            var lambda = Expression.Lambda<Func<TInstance, TParam1, TParam2, TParam3, TParam4, TResult>>(call, instanceParam, param1, param2, param3, param4);
            return lambda.Compile();
        }





        #endregion

        #region CreateMethodAction(Instance)

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Action<TInstance> CreateMethodAction<TInstance>(MethodInfo method)
        {
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var call = Expression.Call(instanceParam, method);
            var lambda = Expression.Lambda<Action<TInstance>>(call, instanceParam);
            return lambda.Compile();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TParam1"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Action<TInstance, TParam1> CreateMethodAction<TInstance, TParam1>(MethodInfo method)
        {
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var param1 = Expression.Parameter(typeof(TParam1), "param1");
            var call = Expression.Call(instanceParam, method, param1);
            var lambda = Expression.Lambda<Action<TInstance, TParam1>>(call, instanceParam, param1);
            return lambda.Compile();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TParam1"></typeparam>
        /// <typeparam name="TParam2"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Action<TInstance, TParam1, TParam2> CreateMethodAction<TInstance, TParam1, TParam2>(MethodInfo method)
        {
            if (method == null) return null;
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var param1 = Expression.Parameter(typeof(TParam1), "param1");
            var param2 = Expression.Parameter(typeof(TParam2), "param2");
            var call = Expression.Call(instanceParam, method, param1, param2);
            var lambda = Expression.Lambda<Action<TInstance, TParam1, TParam2>>(call, instanceParam, param1, param2);
            return lambda.Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TParam1"></typeparam>
        /// <typeparam name="TParam2"></typeparam>
        /// <typeparam name="TParam3"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Action<TInstance, TParam1, TParam2, TParam3> CreateMethodAction<TInstance, TParam1, TParam2, TParam3>(MethodInfo method)
        {
            if (method == null) return null;
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var param1 = Expression.Parameter(typeof(TParam1), "param1");
            var param2 = Expression.Parameter(typeof(TParam2), "param2");
            var param3 = Expression.Parameter(typeof(TParam3), "param3");
            var call = Expression.Call(instanceParam, method, param1, param2, param3);
            var lambda = Expression.Lambda<Action<TInstance, TParam1, TParam2, TParam3>>(call, instanceParam, param1, param2, param3);
            return lambda.Compile();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TParam1"></typeparam>
        /// <typeparam name="TParam2"></typeparam>
        /// <typeparam name="TParam3"></typeparam>
        /// <typeparam name="TParam4"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Action<TInstance, TParam1, TParam2, TParam3, TParam4> CreateMethodAction<TInstance, TParam1, TParam2, TParam3, TParam4>(MethodInfo method)
        {
            if (method == null) return null;
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var param1 = Expression.Parameter(typeof(TParam1), "param1");
            var param2 = Expression.Parameter(typeof(TParam2), "param2");
            var param3 = Expression.Parameter(typeof(TParam3), "param3");
            var param4 = Expression.Parameter(typeof(TParam4), "param4");
            var call = Expression.Call(instanceParam, method, param1, param2, param3, param4);
            var lambda = Expression.Lambda<Action<TInstance, TParam1, TParam2, TParam3, TParam4>>(call, instanceParam, param1, param2, param3, param4);
            return lambda.Compile();
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
