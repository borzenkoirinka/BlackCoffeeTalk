using BlackCoffeeTalk.Shared;
using Microsoft.OData;
using System;
using System.Web.OData.Builder;

namespace BlackCoffeeTalk.Service
{
    public class Configuration
    {
        public static void Register(IContainerBuilder container)
        {
            container.AddService(ServiceLifetime.Scoped, sp =>
            {
                var appConfig = sp.GetService<ApplicationConfiguration>();
                return new BlackCoffeeTalkDbContext(appConfig.MainDatabase);
            });
        
        }

        public static void ConfigureModel(ODataConventionModelBuilder builder)
        {
        }
    }
}
