using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



namespace Magnet
{
    public class TrackerColllection : IEnumerable<ReferenceTracker>, IEnumerable
    {

        // 使用 List 来存储 ReferenceTracker 实例
        private readonly List<ReferenceTracker> _trackers = new List<ReferenceTracker>();
        private readonly Object lockedObject = new Object();


        public void AddRange(IEnumerable<Object> trackObjects)
        {
            lock (lockedObject)
            {
                if (trackObjects != null && trackObjects.Count() > 0)
                {
                    this._trackers.AddRange(trackObjects.Select(trackObject => new ReferenceTracker(trackObject)));
                }
            }

        }

        public void Add(Object trackObject)
        {
            lock (lockedObject)
            {
                if (trackObject != null) this._trackers.Add(new ReferenceTracker(trackObject));
            }

        }


        public Int32 AliveCount
        {
            get
            {
                lock (lockedObject)
                {
                    return this._trackers.Where(E => E.IsAlive).Count();
                }
            }
        }

        public void Check()
        {
            lock (lockedObject) this._trackers.RemoveAll(E => !E.IsAlive);
        }


        public IEnumerable<ReferenceTracker> AliveObjects()
        {
            lock (lockedObject)
            {
                this._trackers.RemoveAll(E => !E.IsAlive);
                return this._trackers;
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
