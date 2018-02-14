using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Owin.Logging;

namespace BlackCoffeeTalk.Hosting
{
    class LoggerEventListener : EventListener
    {
        private readonly ILogger _logger;

        public LoggerEventListener(ILogger logger)
        {
            _logger = logger;
        }
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            string eventDataMessage = eventData.Message;
            if (!string.IsNullOrEmpty(eventData.Message))
            {
                byte maxNumOfParameters = 0;
                for (byte i = 0; i < eventData.PayloadNames.Count; i++)
                {
                    maxNumOfParameters = eventDataMessage.Contains($"{{{i}}}") ? i : maxNumOfParameters;
                }
                if (maxNumOfParameters + 1 > eventData.Payload.Count)
                {
                    eventDataMessage = eventData.Payload.Count == 1 ? eventDataMessage.Replace($"{{{maxNumOfParameters}}}","{0}") : eventDataMessage;
                }
            }
            string message = $"{eventData.Level}: {eventData.EventName}: {string.Format(eventDataMessage ?? string.Empty, eventData.Payload.ToArray())}";
            switch (eventData.Level)
            {
                case EventLevel.Critical:
                    _logger.WriteCritical(message);
                    break;
                case EventLevel.Error:
                    _logger.WriteError(message);
                    break;
                case EventLevel.Informational:
                    _logger.WriteInformation(message);
                    break;
                case EventLevel.Verbose:
                    _logger.WriteVerbose(message);
                    break;
                case EventLevel.Warning:
                    _logger.WriteWarning(message);
                    break;
                default:
                    _logger.WriteInformation(message);
                    break;
            }
        }
    }
}
