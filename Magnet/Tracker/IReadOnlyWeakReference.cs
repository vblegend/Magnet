

namespace Magnet
{

    /// <summary>
    /// Read-only weak reference object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyWeakReference<T>
    {

        /// <summary>
        /// Attempts to get the reference object <br/>
        /// if null is returned the object has been GC
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool TryGetTarget(out T target);

        /// <summary>
        /// Gets the state of the referenced object. <br/>
        /// A return value of false indicates that the referenced object has been GC
        /// </summary>
        public bool IsAlive
        {
            get;
        }

    }
}
