using System;
using System.Diagnostics.Tracing;
using System.Linq;

namespace BlackCoffeeTalk.Hosting.Local
{
    class ConsoleEventListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Console.WriteLine($"{eventData.Level}: {eventData.EventName}: { string.Format(eventData.Message??string.Empty, eventData.Payload.ToArray())}");
        }
    }
}
