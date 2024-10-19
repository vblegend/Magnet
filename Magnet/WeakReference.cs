using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Magnet
{

    /// <summary>
    /// Weak reference to the object 
    /// Same functionality as WeakReference
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WeakReference222<T> where T : class
    {
        private GCHandle _handle;

        public WeakReference222(T target)
        {
            if (target != null)
            {
                _handle = GCHandle.Alloc(target, GCHandleType.Weak);
            }
        }

        public void SetTarget(T target)
        {
            if (_handle.IsAllocated) _handle.Free();
            _handle = GCHandle.Alloc(target, GCHandleType.Weak);
        }




        ~WeakReference222()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }


        public Boolean TryGetTarget(out T target)
        {
            var o = _handle.Target;
            target = o as T;
            return target != null;
        }



        public T Target
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

        public bool IsAlive
        {
            get
            {
                return _handle.IsAllocated && _handle.Target != null;
            }
        }

        public void Dispose()
        {
            if (_handle.IsAllocated) _handle.Free();
        }
    }

}
