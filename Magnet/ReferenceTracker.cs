using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace Magnet
{
    public class ReferenceTracker
    {
        private GCHandle _handle;

        public ReferenceTracker(Object target)
        {
            if (target != null)
            {
                _handle = GCHandle.Alloc(target, GCHandleType.Weak);
            }
        }

        public void SetTarget(Object target)
        {
            if (_handle.IsAllocated) _handle.Free();
            _handle = GCHandle.Alloc(target, GCHandleType.Weak);
        }




        ~ReferenceTracker()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTarget([MaybeNullWhen(false), NotNullWhen(true)] out Object target)
        {
            Object? o = this.Target;
            target = o!;
            return o != null;
        }

        public Object Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_handle.IsAllocated)
                {
                    return _handle.Target ;
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
