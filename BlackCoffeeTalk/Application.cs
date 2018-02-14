using Owin;
using Microsoft.Owin.Cors;
using ApplicationV2 = BlackCoffeeTalk.Framework.Service;              
using BlackCoffeeTalk.Shared;
using System.Diagnostics.Tracing;
using System.Web.Http;
using BlackCoffeeTalk.Hosting;
using Microsoft.Owin.Logging;
using NLog.Owin.Logging;
using Swashbuckle.Application;

namespace BlackCoffeeTalk
{
    public class Application
    {
        public static IAppBuilder Bootstrap(IAppBuilder app, ApplicationConfiguration config)
        {
            config.Register = BlackCoffeeTalk.Service.Configuration.Register;
            config.ConfigureModel = BlackCoffeeTalk.Service.Configuration.ConfigureModel;
            app.Properties[ApplicationConfiguration.Key] = config;

            app.UseCors(CorsOptions.AllowAll);            
            
            app.UseNLog();
            var logger = app.CreateLogger<Application>();            
            var loggerEventListener = new LoggerEventListener(logger);
            loggerEventListener.EnableEvents(config.EventSource, EventLevel.LogAlways, (EventKeywords)long.MaxValue);

            app.Map("/v2", ApplicationV2.Configure);

            app.Map("", ConfigureSwagger);

            return app;
        }

        private static void ConfigureSwagger(IAppBuilder app)
        {
            var http = new HttpConfiguration();

            var swaggerUiConfig = new SwaggerUiConfig(new[] { "v2" }, m => "");
            swaggerUiConfig.EnableDiscoveryUrlSelector();
            
            //swaggerUiConfig.EnableApiKeySupport("Authorization", "header");
            //swaggerUiConfig.InjectJavaScript(typeof(Application).Assembly, "BlackCoffeeTalk.Bootstrap.ClientResources.swaggerExtensions.js");

            http.Routes.MapHttpRoute("swagger_ui_shortcut", "", null, null, new RedirectHandler(r => r.RequestUri.AbsoluteUri.TrimEnd('/'), "index"));
            http.Routes.MapHttpRoute("swagger_ui", "{*assetPath}", null, new { assetPath = ".+" }, new SwaggerUiHandler(swaggerUiConfig));


            app.UseWebApi(http);
        }
    }
}
