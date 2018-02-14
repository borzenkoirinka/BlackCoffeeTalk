using Microsoft.OData;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Web.OData.Builder;

namespace BlackCoffeeTalk.Shared
{
    public class ApplicationConfiguration
    {
        public static readonly string Key = "blackCoffeeTalk-configuration";
        public string MainDatabase { get; set; }
        public ApplicationEventSource EventSource { get; set; }
        public string HostAddress { get; set; }

        public Action<ODataConventionModelBuilder> ConfigureModel { get; set; }

        public Action<IContainerBuilder> Register { get; set; }
    }
}
