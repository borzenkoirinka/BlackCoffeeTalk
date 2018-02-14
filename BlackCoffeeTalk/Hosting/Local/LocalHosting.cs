using BlackCoffeeTalk.Shared;
using Microsoft.Owin.Hosting;
using System;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace BlackCoffeeTalk.Hosting.Local
{
    internal sealed class LocalHosting : IHosting, ILocalSettings
    {
        private IConfigurationRoot Configuration { get; }

        public string ConnectionString => Configuration.GetConnectionString("MainDBConnectionString");

        public string HostAddress => Configuration.GetSection("hosting")["hostAddress"];
        
        public LocalHosting(ApplicationEventSource log, IConfigurationRoot configuration)
        {
            this.Configuration = configuration;
            HostApplication(log);
        }

        public void HostApplication(ApplicationEventSource log)
        {
            var eventListener = new ConsoleEventListener();
            eventListener.EnableEvents(log, EventLevel.LogAlways, (EventKeywords)long.MaxValue);
           
            var config = new ApplicationConfiguration
            {
                EventSource = log,
                MainDatabase = this.ConnectionString,
                HostAddress = this.HostAddress,
            };

            using (WebApp.Start(this.HostAddress, app => Application.Bootstrap(app, config)))
            {
                log.Message($"Local service started at {this.HostAddress}");
                Task.Delay(-1).Wait();
            }
        }
    }
}
