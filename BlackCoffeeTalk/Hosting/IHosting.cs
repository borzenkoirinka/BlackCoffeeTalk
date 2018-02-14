using BlackCoffeeTalk.Shared;

namespace BlackCoffeeTalk.Hosting
{
    internal interface IHosting
    {
        void HostApplication(ApplicationEventSource log);
    }
}