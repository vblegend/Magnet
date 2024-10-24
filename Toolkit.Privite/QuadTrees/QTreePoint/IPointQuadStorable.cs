

using Toolkit.Private.QuadTrees.Common;
using System.Drawing;

namespace Toolkit.Private.QuadTrees.QTreePoint
{
    /// <summary>
    /// Interface to define Rect, so that QuadTree knows how to store the object.
    /// </summary>
    public interface IPointQuadStorable
    {
        /// <summary>
        /// The PointF that defines the object's boundaries.
        /// </summary>
        Point Point { get; }
    }
}