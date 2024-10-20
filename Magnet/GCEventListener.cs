using System;
using System.Diagnostics.Tracing;

namespace Magnet
{
    internal class GCEventListener : EventListener
    {

        public event Action OnGCFinalizers;


        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            // Listen for.NET Runtime GC events
            if (eventSource.Name == "Microsoft-Windows-DotNETRuntime")
            {
                // Enable GC-related event listening (0x1 = GC-related event)
                EnableEvents(eventSource, EventLevel.Informational, (EventKeywords)0x1);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventName == "GCFinalizersEnd_V1")
            {
                this.OnGCFinalizers?.Invoke();
            }
        }
    }

}
