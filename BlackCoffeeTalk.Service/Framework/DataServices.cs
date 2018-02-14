using BlackCoffeeTalk.Framework.Components;
using BlackCoffeeTalk.Framework.DependencyInjection;
using BlackCoffeeTalk.Framework.Infrastructure;
using BlackCoffeeTalk.Framework.Infrastructure.Conventions;
using BlackCoffeeTalk.Shared;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query.Expressions;
using System.Web.OData.Routing.Conventions;
using BlackCoffeeTalk.Framework.Common;
using BlackCoffeeTalk.Framework;
using System;

namespace BlackCoffeeTalk.Framework
{
    class DataServices
    {
        internal static void Configure(HttpConfiguration config, ApplicationConfiguration appConfig)
        {
            config.Services.Replace(typeof(IHttpControllerActivator), new ODataControllerActivator());

            config.MapODataServiceRoute("odata", "api", icb => BuildContainer(icb, appConfig, config));
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        private static void BuildContainer(IContainerBuilder container, ApplicationConfiguration appConfig, HttpConfiguration config)
        {
            container.AddService(ServiceLifetime.Singleton, sp => appConfig);
            container.AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new UnqualifiedODataUriResolver());
            
            container.AddService(ServiceLifetime.Singleton, sp =>
            {
                var conventions = ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config);
                conventions.Insert(0, new UnboundFunctionsConvention());
                //conventions.Insert(0, new AnyNavigationRoutingConvension());
                conventions.Insert(0, new SingleEntityRoutingConvention());
                return conventions.AsEnumerable();
            });

            var controllers = appConfig.ConfigureModel.Method.DeclaringType.Assembly.ExportedTypes
                .Where(t => !t.IsAbstract)
                .Where(typeof(ODataController).IsAssignableFrom).ToList();
            container.AddService<MetadataController>(ServiceLifetime.Transient);
            container.AddService<ExtendedMetadataController>(ServiceLifetime.Transient);

            container.AddService<FilterBinder, ExtendedFilterBinder>(ServiceLifetime.Singleton);
            ExtendedFilterBinder.RegisterCustomFunctions();

            var builder = new ODataConventionModelBuilder();
            builder.Namespace = typeof(Service).Namespace;
            builder.EnableLowerCamelCase();
            builder.ContainerName = "DefaultContainer";
            var entitySetFactory = builder.GetType().GetMethod("EntitySet");

            foreach (var controller in controllers)
            {
                container.AddService(ServiceLifetime.Transient, controller);

                var modelType = GetControllerBaseModel(controller);

                if (modelType!=null)
                {
                    var setName = controller.Name.Replace("Controller", "");
                    entitySetFactory.MakeGenericMethod(modelType).Invoke(builder, new object[] { setName });
                }
            }


            
            builder.Function(Constants.ValidationFunctionName).Returns<ValidationEntityDocument>();
            //builder.Function(Constants.GenerateFunctionName).Returns<IHttpActionResult>(); 
            appConfig.ConfigureModel(builder);
            appConfig.Register(container);

            var model = builder.GetEdmModel();

            container.AddService(ServiceLifetime.Singleton, sp => model);
        }

        static Type GetControllerBaseModel(Type controllerType)
        {
            var baseType = controllerType;

            while (baseType != null && !(baseType.IsGenericType/* && baseType.GetGenericTypeDefinition() == typeof(ControllerBase<>)*/))
            {
                baseType = baseType.BaseType;
            }

            return baseType?.GetGenericArguments()[0];
        }
    }
}
