using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



namespace Magnet.Tracker
{
    public class TrackerColllection : IEnumerable<ReferenceTracker>, IEnumerable
    {

        // 使用 List 来存储 ReferenceTracker 实例
        private readonly List<ReferenceTracker> _trackers = new List<ReferenceTracker>();
        private readonly object lockedObject = new object();


        public void AddRange(IEnumerable<object> trackObjects)
        {
            lock (lockedObject)
            {
                if (trackObjects != null && trackObjects.Count() > 0)
                {
                    _trackers.AddRange(trackObjects.Select(trackObject => new ReferenceTracker(trackObject)));
                }
            }

        }

        public void Add(object trackObject)
        {
            lock (lockedObject)
            {
                if (trackObject != null) _trackers.Add(new ReferenceTracker(trackObject));
            }

        }


        public int AliveCount
        {
            get
            {
                lock (lockedObject)
                {
                    return _trackers.Where(E => E.IsAlive).Count();
                }
            }
        }

        public void Check()
        {
            lock (lockedObject) _trackers.RemoveAll(E => !E.IsAlive);
        }


        public IEnumerable<ReferenceTracker> AliveObjects()
        {
            lock (lockedObject)
            {
                _trackers.RemoveAll(E => !E.IsAlive);
                return _trackers;
            }
        }



        public IEnumerator<ReferenceTracker> GetEnumerator()
        {
            lock (lockedObject) return _trackers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (lockedObject) return _trackers.GetEnumerator();
        }
    }
}
