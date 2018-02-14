using BlackCoffeeTalk.Framework.Infrastructure;
using Microsoft.Owin;
using Swashbuckle.Application;
using Swashbuckle.OData;
using System;
using System.Net.Http;
using System.Web.Http;

namespace BlackCoffeeTalk.Framework
{
    class Documentation
    {
        internal static void Configure(HttpConfiguration config)
        {
            var swConfig = new SwaggerDocsConfig();
            swConfig.RootUrl(m => GetOwinAppBasePath(m.GetOwinContext()));
            swConfig.OperationFilter(() => new ODataResponcesOperationFilter());
            swConfig.OperationFilter(() => new AddExtendedMetadataOperationFilter());
            swConfig.DocumentFilter(() => new ODataRemoveNotAllowedOperationFilter());
            swConfig.SingleApiVersion("v2", "BlackCoffeeTalk oData Services");
            swConfig.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, swConfig, config)
            .Configure(odatasw => {
                odatasw.EnableSwaggerRequestCaching();
                odatasw.IncludeNavigationProperties();
            }));
            swConfig.ApiKey("Token")
                .Description("Filling bearer token.")
                .Name("Authorization")
                .In("header");

            config.Routes.MapHttpRoute("swagger_docs", "", new { apiVersion = "v2" }, null, new SwaggerDocsHandler(swConfig));            
        }

        private static string GetOwinAppBasePath(IOwinContext ctx)
        {
            return ctx.Request.Scheme + Uri.SchemeDelimiter + ctx.Request.Host + ctx.Request.PathBase;
        }
    }
}
