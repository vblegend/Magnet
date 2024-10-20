using System;
using System.Collections.Generic;


namespace App.Core.Trees
{

    public interface ILocalizable
    {
        Int32 X { get; }
        Int32 Y { get; }
    }

    public struct Rectangle
    {
        public Int32 Left { get; }
        public Int32 Top { get; }
        public Int32 Right { get; }
        public Int32 Bottom { get; }

        public Rectangle(Int32 left, Int32 top, Int32 right, Int32 bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public bool Contains(Int32 x, Int32 y)
        {
            return x >= Left && y >= Top && x < Right && y < Bottom;
        }
    }

    public class QuadTree<T> where T : ILocalizable
    {
        private readonly Int32 _x;
        private readonly Int32 _y;
        private readonly Int32 _width;
        private readonly Int32 _height;
        private readonly List<T> _items;
        private QuadTree<T>[] _nodes;

        public QuadTree(Int32 x, Int32 y, Int32 width, Int32 height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _items = new List<T>();
        }

        public void Insert(T item)
        {
            if (!Contains(item.X, item.Y)) return;

            if (_items.Count < 4 || Math.Abs(_width) <= 1 || Math.Abs(_height) <= 1)
            {
                _items.Add(item);
                return;
            }

            if (_nodes == null) Subdivide();

            foreach (var node in _nodes)
            {
                node.Insert(item);
            }
        }

        public void Move(T item, Int32 oldX, Int32 oldY)
        {
            if (Contains(oldX, oldY))
            {
                Remove(item);
                Insert(item);
            }
        }

        public void Remove(T item)
        {
            if (!Contains(item.X, item.Y)) return;

            if (_items.Remove(item)) return;

            if (_nodes != null)
            {
                foreach (var node in _nodes)
                {
                    node.Remove(item);
                }
            }
        }


        public IEnumerable<T> Query(Int32 x, Int32 y)
        {
            if (!Intersects(x, y)) yield break;

            foreach (var item in _items)
            {
                if (item.X == x && item.Y == y)
                {
                    yield return item;
                }
            }

            if (_nodes != null)
            {
                foreach (var node in _nodes)
                {
                    foreach (var item in node.Query(x, y))
                    {
                        yield return item;
                    }
                }
            }
        }

        public IEnumerable<T> Query(Rectangle rect)
        {
            if (!Intersects(rect)) yield break;

            foreach (var item in _items)
            {
                if (rect.Contains(item.X, item.Y))
                {
                    yield return item;
                }
            }

            if (_nodes != null)
            {
                foreach (var node in _nodes)
                {
                    foreach (var item in node.Query(rect))
                    {
                        yield return item;
                    }
                }
            }
        }


        private void Subdivide()
        {
            var halfWidth = _width / 2;
            var halfHeight = _height / 2;
            _nodes = new[]
            {
                new QuadTree<T>(_x, _y, halfWidth, halfHeight),
                new QuadTree<T>(_x + halfWidth, _y, halfWidth, halfHeight),
                new QuadTree<T>(_x, _y + halfHeight, halfWidth, halfHeight),
                new QuadTree<T>(_x + halfWidth, _y + halfHeight, halfWidth, halfHeight),
            };
            foreach (var item in _items)
            {
                foreach (var node in _nodes)
                {
                    node.Insert(item);
                }
            }
            _items.Clear();
        }

        private bool Contains(Int32 x, Int32 y)
        {
            return x >= _x && y >= _y && x < _x + _width && y < _y + _height;
        }


        private bool Intersects(Int32 x, Int32 y)
        {
            return x < _x + _width && x > _x && y < _y + _height && y > _y;
        }



        private bool Intersects(Rectangle rect)
        {
            return rect.Left < _x + _width && rect.Right > _x && rect.Top < _y + _height && rect.Bottom > _y;
        }
    }


}
