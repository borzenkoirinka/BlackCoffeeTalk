using BlackCoffeeTalk.Shared;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using Microsoft.Diagnostics.EventFlow.ServiceFabric;
//using Microsoft.Diagnostics.EventFlow;
using BlackCoffeeTalk.Hosting;
using BlackCoffeeTalk.Hosting.Asf;

namespace BlackCoffeeTalk.Hosting.ASF
{
    internal sealed class ServiceFactoryHosting: IHosting
    {
        public ServiceFactoryHosting(ApplicationEventSource log)
        {
            HostApplication(log);
        }
        public void HostApplication(ApplicationEventSource log)
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                //using (DiagnosticPipeline diagnosticsPipeline = ServiceFabricDiagnosticPipelineFactory.CreatePipeline("Service-DiagnosticsPipeline"))
                {

                    ServiceRuntime.RegisterServiceAsync("BlackCoffeeTalkType",
                    context => new BlackCoffeeTalk.Hosting.Asf.Service(context)).GetAwaiter().GetResult();

                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(BlackCoffeeTalk.Hosting.Asf.Service).Name);

                    Thread.Sleep(Timeout.Infinite);
                }
                Task.Delay(-1).Wait();
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
