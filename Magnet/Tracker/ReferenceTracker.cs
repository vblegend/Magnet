using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace Magnet.Tracker
{
    /// <summary>
    /// 
    /// </summary>
    internal class ReferenceTracker
    {
        private GCHandle _handle;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public ReferenceTracker(object target)
        {
            if (target != null)
            {
                _handle = GCHandle.Alloc(target, GCHandleType.Weak);
            }
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
        ~ReferenceTracker()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTarget([MaybeNullWhen(false), NotNullWhen(true)] out object target)
        {
            object o = Target;
            target = o!;
            return o != null;
        }


        /// <summary>
        /// 
        /// </summary>
        public object Target
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
    }
}
