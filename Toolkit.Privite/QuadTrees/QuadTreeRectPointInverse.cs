﻿using Game.Toolkit.QuadTrees.Common;
using Game.Toolkit.QuadTrees.QTreeRect;
using System.Drawing;

namespace Game.Toolkit.QuadTrees
{
    /// <summary>
    /// A QuadTree Object that provides fast and efficient storage of Rectangles in a world space, queried using Rectangles.
    /// </summary>
    /// <typeparam name="T">Any object implementing IQuadStorable.</typeparam>
    public class QuadTreeRectPointInverse<T> : QuadTreeCommon<T, QuadTreeRectNode<T, Point>, Point> where T : IRectQuadStorable
    {
        public QuadTreeRectPointInverse(Rectangle rect) : base(rect)
        {
        }

        public QuadTreeRectPointInverse(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
        }

        protected override QuadTreeRectNode<T, Point> CreateNode(Rectangle rect)
        {
            return new QuadTreeRectPointInvNode<T>(rect);
        }
    }
}
