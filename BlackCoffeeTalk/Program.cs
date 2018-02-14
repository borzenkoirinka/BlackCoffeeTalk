using BlackCoffeeTalk.Shared;
using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Threading;
using System.Diagnostics;
using BlackCoffeeTalk.Hosting.Asf;

namespace BlackCoffeeTalk
{
    public static class Program
    {
        public static IConfigurationRoot Configuration { get; }

        static Program()
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("BLACKCOFFEETALK_ENVIRONMENT")}.json", //
                    optional: true);

            Configuration = builder.Build();
        }

        private static void Main()
        {
            var hostingType = Configuration.GetSection("hosting")["hostingType"];
            Console.WriteLine(hostingType);
            var hosting = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.DefinedTypes)
                .Where(t => t.AssemblyQualifiedName.Contains(hostingType)).FirstOrDefault(t => t.Name == hostingType);

            object[] constructorParams = { new ApplicationEventSource(), Configuration };
            var constructor = hosting.GetConstructors().FirstOrDefault().GetParameters().Select(t => t.ParameterType);
            constructorParams = constructorParams
                .Where(t => constructor.Any(z => z == t.GetType() ||
                                                 ((System.Reflection.TypeInfo)t.GetType()).ImplementedInterfaces
                                                 .Contains(z))).ToArray();
            Activator.CreateInstance(hosting, constructorParams);
        }
        //private static void Main()
        //{
        //    try
        //    {
        //        // The ServiceManifest.XML file defines one or more service type names.
        //        // Registering a service maps a service type name to a .NET type.
        //        // When Service Fabric creates an instance of this service type,
        //        // an instance of the class is created in this host process.

        //        ServiceRuntime.RegisterServiceAsync("BlackCoffeeTalkType",
        //            context => new BlackCoffeeTalk.Hosting.Asf.Service(context)).GetAwaiter().GetResult();

        //        ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(Hosting.Asf.Service).Name);

        //        // Prevents this host process from terminating so services keep running.
        //        Thread.Sleep(Timeout.Infinite);
        //    }
        //    catch (Exception e)
        //    {
        //        ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
        //        throw;
        //    }
        //}
    }
}
