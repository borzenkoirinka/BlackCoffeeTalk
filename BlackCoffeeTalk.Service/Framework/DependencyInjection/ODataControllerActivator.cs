using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;

namespace BlackCoffeeTalk.Framework.DependencyInjection
{
    public class ODataControllerActivator : IHttpControllerActivator
    {
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {          
            var scopeFactory = request.GetRequestContainer().GetRequiredService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            request.RegisterForDispose(scope);
          
            var controller = scope.ServiceProvider.GetService(controllerType) as IHttpController;
            return controller;
        }
    }
}
