using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Magnet.Tracker
{
    internal class ReferenceTracker
    {
        private GCHandle _handle;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public ReferenceTracker(Object target)
        {
            if (target != null)
            {
                _handle = GCHandle.Alloc(target, GCHandleType.Weak);
            }
        }

        internal ReferenceTracker(GCHandle handle)
        {
            _handle = handle;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(object target)
        {
            if (_handle.IsAllocated) _handle.Free();
            _handle = GCHandle.Alloc(target, GCHandleType.Weak);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTarget<T>([MaybeNullWhen(false), NotNullWhen(true)] out T target) where T : class
        {
            Object o = Target;
            target = (T)o!;
            return o != null;
        }


        /// <summary>
        /// 
        /// </summary>
        public Object Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_handle.IsAllocated)
                {
                    return _handle.Target;
                }
                return null;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return _handle.IsAllocated && _handle.Target != null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_handle.IsAllocated) _handle.Free();
        }

        /// <summary>
        /// 
        /// </summary>
        ~ReferenceTracker()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }

        public ReferenceTracker<T> As<T>() where T : class
        {
            return new ReferenceTracker<T>(_handle);
        }
    }


    internal sealed class ReferenceTracker<T> : IReadOnlyWeakReference<T> where T : class
    {
        private readonly GCHandle _handle;

        internal ReferenceTracker(GCHandle handle)
        {
            this._handle = handle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTarget([MaybeNullWhen(false), NotNullWhen(true)] out T target)
        {
            Object o = Target;
            target = (T)o!;
            return o != null;
        }


        /// <summary>
        /// 
        /// </summary>
        private T Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_handle.IsAllocated)
                {
                    return _handle.Target as T;
                }
                return null;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public bool IsAlive
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _handle.IsAllocated && _handle.Target != null;
            }
        }


    }


}
