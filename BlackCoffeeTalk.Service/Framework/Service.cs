using BlackCoffeeTalk.Framework.Infrastructure;
using BlackCoffeeTalk.Shared;
using MultipartDataMediaFormatter;
using MultipartDataMediaFormatter.Infrastructure;
using Owin;
using System;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace BlackCoffeeTalk.Framework
{
    public class Service
    {
        public static void Configure(IAppBuilder app)
        {
            var appConfig = (ApplicationConfiguration)app.Properties[ApplicationConfiguration.Key];
            var httpConfig = new HttpConfiguration();
            var conf = (ApplicationConfiguration)app.Properties[ApplicationConfiguration.Key];

            httpConfig.Filters.Add(new ServiceRequestActionFilterAttribute(conf.EventSource));
            httpConfig.Filters.Add(new BlackCoffeeTalk.Framework.Infrastructure.ExceptionHandler(conf.EventSource));
            httpConfig.Services.Replace(typeof(IExceptionHandler), new GlobalExceptionHandler(conf.EventSource));
            //httpConfig.Filters.Add(new ValidateModelAttribute());
            httpConfig.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            httpConfig.Formatters.Add(new FormMultipartEncodedMediaTypeFormatter(new MultipartFormatterSettings()));

           // Security.AuthServerV1Configure(httpConfig, appConfig, app);
            DataServices.Configure(httpConfig, appConfig);
            Documentation.Configure(httpConfig);

            httpConfig.EnsureInitialized();
            app.UseWebApi(httpConfig);
        }
    }
}
